using System.Data;
using System.Text.Json;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class ProductDetailService(DispatcherDbContext db, ProductListService products, CatalogService catalogs)
{
    private async Task<ProductListDto> ProductAsync(long id, CancellationToken ct)
    {
        var result = await products.ListAsync(new ProductListQuery
        {
            Filters = [new() { Field = "id", Operator = ProductFilterOperator.Eq, Value = id.ToString(System.Globalization.CultureInfo.InvariantCulture) }],
        }, ct);
        return result.Items.SingleOrDefault() ?? throw new KeyNotFoundException();
    }

    public async Task<ProductDetailDto> GetAsync(long id, CancellationToken ct)
    {
        var product = await ProductAsync(id, ct);
        var history = new List<ProductHistoryEntry>();
        history.AddRange(await db.ProductAssignments.AsNoTracking().Where(x => x.ProductId == id)
            .Select(x => new ProductHistoryEntry("Назначение на объект", x.Id, x.ValidFrom, x.ValidTo,
                x.ConstructionObject.Name + " / " + (x.BuildingSection == null ? "—" : x.BuildingSection.Name)
                + " / " + (x.Floor == null ? "—" : x.Floor.Name) + " / " + (x.InstallationNumber ?? "—"), x.Comment)).ToListAsync(ct));
        history.AddRange(await db.StoragePlacements.AsNoTracking().Where(x => x.ProductId == id)
            .Select(x => new ProductHistoryEntry("Склад", x.Id, x.ArrivedAt, x.DepartedAt, x.StorageArea.Name, x.Comment)).ToListAsync(ct));
        history.AddRange(await db.TripItems.AsNoTracking().Where(x => x.ProductId == id)
            .Select(x => new ProductHistoryEntry("Рейс", x.Id, x.AddedAt, x.Trip.ActualArrivalAt,
                x.Trip.TripNumber + " / " + x.Trip.ConstructionObject.Name + " / " + x.Trip.Vehicle.RegistrationNumber, x.Trip.Comment)).ToListAsync(ct));
        history.AddRange(await db.TaktAssignments.AsNoTracking().Where(x => x.ProductId == id)
            .Select(x => new ProductHistoryEntry("Такт", x.Id, x.ValidFrom, x.ValidTo, x.ConstructionTakt.Name, x.Comment)).ToListAsync(ct));
        var revisions = await db.DemandRevisions.AsNoTracking().Where(x => x.Demand.ProductId == id)
            .Select(x => new { x.Id, x.CreatedAt, x.RevisionNumber, x.RequiredDeliveryDate, x.RequiredProductionStartDate,
                x.RequiredProductionEndDate, x.EarliestDeliveryDate, x.PriorityLevel, x.PriorityOrder, x.ChangeReason, x.Comment,
                Takt = x.ConstructionTakt.Name }).ToListAsync(ct);
        history.AddRange(revisions.Select(x => new ProductHistoryEntry("Версия потребности", x.Id, x.CreatedAt, null,
            $"{x.Takt}, версия {x.RevisionNumber}; производство {x.RequiredProductionStartDate:dd.MM.yyyy}–{x.RequiredProductionEndDate:dd.MM.yyyy}; "
            + $"доставка {x.EarliestDeliveryDate:dd.MM.yyyy}–{x.RequiredDeliveryDate:dd.MM.yyyy}; приоритет {x.PriorityLevel}, порядок {x.PriorityOrder}",
            string.Join("; ", new[] { x.ChangeReason, x.Comment }.Where(x => !string.IsNullOrWhiteSpace(x))))));
        var production = await db.ProductionAssignments.AsNoTracking().Where(x => x.DemandRevision.Demand.ProductId == id)
            .Select(x => new { x.Id, x.ValidFrom, x.ValidTo, x.PlannedProductionDate, x.Status, x.AssignmentMethod,
                Line = x.ProductionLine.Name, Plant = x.ProductionLine.Plant.Name, x.OverrideReason }).ToListAsync(ct);
        history.AddRange(production.Select(x => new ProductHistoryEntry("Производственное назначение", x.Id, x.ValidFrom, x.ValidTo,
            $"{x.Plant} / {x.Line}; {x.PlannedProductionDate:dd.MM.yyyy}; {x.Status}; {x.AssignmentMethod}", x.OverrideReason)));
        var entityId = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        history.AddRange(await db.AuditEvents.AsNoTracking().Where(x => x.EntityType == nameof(Product)
                && x.EntityId == entityId && (x.Action == "EditProductDetails" || x.Action == "EditProductIdentifier"
                    || x.Action == "RevokeProductIdentifier" || x.Action == "ReviseProductDemand" || x.Action == "CancelProductDemand"))
            .Select(x => new ProductHistoryEntry("Редактирование карточки", x.Id, x.Timestamp, null, x.Comment ?? "Изменение изделия", null)).ToListAsync(ct));
        return new(product, history.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToList());
    }

    public async Task<IReadOnlyList<ProductTripOption>> TripsAsync(long id, int? objectId, CancellationToken ct)
    {
        var product = await ProductAsync(id, ct);
        var destination = objectId ?? product.ConstructionObjectId;
        if (destination == null) return [];
        return await db.Trips.AsNoTracking()
            .Where(x => x.ConstructionObjectId == destination && x.ConstructionObject.IsActive && x.Plant.IsActive && x.Vehicle.IsActive
                && (x.Status == TripStatus.Draft || x.Status == TripStatus.Planned)
                && x.ActualLoadingAt == null && x.ActualDepartureAt == null && x.ActualArrivalAt == null)
            .OrderBy(x => x.PlannedArrivalAt).ThenBy(x => x.Id)
            .Select(x => new ProductTripOption(x.Id, x.TripNumber, x.Status.ToString(), x.Plant.Name,
                x.Vehicle.RegistrationNumber, x.PlannedArrivalAt)).ToListAsync(ct);
    }

    public async Task<ProductDetailDto> UpdateAsync(long id, ProductDetailWriteDto dto, string? userId, CancellationToken ct)
    {
        ObjectProductsService.Validate(dto);
        // A single transaction also protects competing edits to the trip manifest and related assignments.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var current = await ProductAsync(id, ct);
        if (dto.Version != current.Version) throw new ConflictException("Изделие изменено другим пользователем. Обновите карточку перед сохранением.");
        if (dto.ProductCode != current.ProductCode) throw new ArgumentException("Код изделия неизменяемый.");
        if (dto.Status != current.Status) throw new ArgumentException("Статус изменяется операциями производства и логистики.");
        if (!await db.ProductTypes.AnyAsync(x => x.Id == dto.ProductTypeId, ct)) throw new ArgumentException("Тип изделия не найден.");

        var placementChanged = dto.ConstructionObjectId != current.ConstructionObjectId || dto.BuildingSectionId != current.BuildingSectionId
            || dto.FloorId != current.FloorId || dto.InstallationNumber != current.InstallationNumber;
        if (placementChanged)
        {
            if (dto.ConstructionObjectId == null) throw new ArgumentException("Выберите объект для назначения изделия.");
            if (current.Status is ProductStatus.InTransit or ProductStatus.Delivered or ProductStatus.Cancelled)
                throw new ConflictException("Расположение изделия в пути, доставленного или отменённого изделия нельзя менять в карточке.");
            if (!await db.ConstructionObjects.AnyAsync(x => x.Id == dto.ConstructionObjectId && x.IsActive, ct))
                throw new ArgumentException("Выбранный объект неактивен или не найден.");
            if (dto.BuildingSectionId.HasValue && !await db.BuildingSections.AnyAsync(x => x.Id == dto.BuildingSectionId && x.ConstructionObjectId == dto.ConstructionObjectId, ct))
                throw new ArgumentException("Секция не принадлежит выбранному объекту.");
            if (dto.FloorId.HasValue && (!dto.BuildingSectionId.HasValue || !await db.Floors.AnyAsync(x => x.Id == dto.FloorId && x.BuildingSectionId == dto.BuildingSectionId, ct)))
                throw new ArgumentException("Этаж не принадлежит выбранной секции.");
            if (dto.ConstructionObjectId != current.ConstructionObjectId &&
                (await db.TaktAssignments.AnyAsync(x => x.ProductId == id && x.ValidTo == null, ct)
                || await db.Demands.AnyAsync(x => x.ProductId == id && x.Status != DemandStatus.Cancelled && x.Status != DemandStatus.Fulfilled, ct)))
                throw new ConflictException("Сначала переназначьте действующий такт и потребности: они связаны с текущим объектом.");
        }

        var product = await db.Products.SingleAsync(x => x.Id == id, ct);
        db.Entry(product).Property(x => x.Version).OriginalValue = dto.Version;
        var activeItems = await db.TripItems.Include(x => x.Trip).Where(x => x.ProductId == id
            && x.Trip.Status != TripStatus.Cancelled && x.Trip.Status != TripStatus.Completed).ToListAsync(ct);
        if (activeItems.Count > 1) throw new ConflictException("У изделия несколько действующих рейсов. Сначала исправьте состав рейсов.");
        var previous = activeItems.SingleOrDefault();
        var tripChanged = dto.TripId != previous?.TripId;
        var next = dto.TripId.HasValue ? await db.Trips.Include(x => x.Vehicle).ThenInclude(x => x.VehicleType)
            .Include(x => x.Plant).Include(x => x.ConstructionObject).SingleOrDefaultAsync(x => x.Id == dto.TripId, ct)
            ?? throw new ArgumentException("Рейс не найден.") : null;
        if (tripChanged)
        {
            if (current.Status is ProductStatus.InTransit or ProductStatus.Delivered or ProductStatus.Cancelled)
                throw new ConflictException("Для текущего состояния изделия назначение рейса недоступно.");
            if (previous != null) RequireEditableTrip(previous.Trip);
            if (next != null) RequireEditableTrip(next);
        }
        if (next != null && (tripChanged || placementChanged || dto.WeightKg != current.WeightKg))
        {
            RequireEditableTrip(next);
            if (next.ConstructionObjectId != dto.ConstructionObjectId) throw new ArgumentException("Рейс направляется на другой объект.");
            if (!next.Vehicle.IsActive || !next.Plant.IsActive || !next.ConstructionObject.IsActive)
                throw new ConflictException("Транспорт, завод или объект рейса неактивен.");
            var storagePlants = await db.StoragePlacements.Where(x => x.ProductId == id && x.DepartedAt == null)
                .Select(x => x.StorageArea.PlantId).ToListAsync(ct);
            if (storagePlants.Any(x => x == null || x != next.PlantId) || (storagePlants.Count == 0 && current.PlantId.HasValue && current.PlantId != next.PlantId))
                throw new ArgumentException("Завод рейса не соответствует месту хранения или производства изделия.");
            var weights = await db.TripItems.Where(x => x.TripId == next.Id && x.ProductId != id).Select(x => x.Product.WeightKg).ToListAsync(ct);
            if (next.Vehicle.VehicleType.MaxPayloadKg.HasValue)
            {
                if (dto.WeightKg == null || weights.Any(x => x == null)) throw new ArgumentException("Укажите массу всех изделий рейса для проверки грузоподъёмности.");
                if (weights.Sum(x => x ?? 0) + dto.WeightKg.Value > next.Vehicle.VehicleType.MaxPayloadKg.Value)
                    throw new ArgumentException("Превышена грузоподъёмность транспорта.");
            }
        }
        var now = DateTimeOffset.UtcNow;
        if (placementChanged)
        {
            var assignments = await db.ProductAssignments.Where(x => x.ProductId == id && x.ValidTo == null).ToListAsync(ct);
            foreach (var assignment in assignments) assignment.ValidTo = now;
            db.ProductAssignments.Add(new ProductAssignment { ProductId = id, ConstructionObjectId = dto.ConstructionObjectId!.Value,
                BuildingSectionId = dto.BuildingSectionId, FloorId = dto.FloorId, InstallationNumber = dto.InstallationNumber,
                ValidFrom = now, Reason = AssignmentReason.Correction, Comment = "Изменение в карточке изделия" });
        }
        if (tripChanged)
        {
            if (previous != null) { db.TripItems.Remove(previous); previous.Trip.Version++; previous.Trip.UpdatedAt = now; }
            if (next != null)
            {
                var sequence = await db.TripItems.Where(x => x.TripId == next.Id).Select(x => (int?)x.LoadingSequence).MaxAsync(ct) ?? 0;
                db.TripItems.Add(new TripItem { TripId = next.Id, ProductId = id, LoadingSequence = sequence + 1, AddedAt = now, AddedByUserId = userId });
                next.Version++; next.UpdatedAt = now;
            }
            // Planning does not imply production or physical movement.
            if (current.Status == ProductStatus.InStorage && next != null) dto.Status = ProductStatus.AssignedToTrip;
            if (current.Status == ProductStatus.AssignedToTrip && next == null)
            {
                if (!await db.StoragePlacements.AnyAsync(x => x.ProductId == id && x.DepartedAt == null, ct))
                    throw new ConflictException("Не найдено текущее размещение изделия. Сначала уточните складской учёт.");
                dto.Status = ProductStatus.InStorage;
            }
        }
        db.AuditEvents.Add(new AuditEvent
        {
            EntityType = nameof(Product), EntityId = id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Action = "EditProductDetails", Timestamp = now, UserId = userId, Source = AuditSource.UI,
            OldValuesJson = JsonSerializer.Serialize(current), NewValuesJson = JsonSerializer.Serialize(dto),
            Comment = tripChanged ? $"Изменение рейса: {previous?.Trip.TripNumber ?? "не назначен"} → {next?.TripNumber ?? "не назначен"}" : "Изменение характеристик или расположения изделия",
        });
        await catalogs.UpdateProductAsync(id, dto, ct);
        await tx.CommitAsync(ct);
        return await GetAsync(id, ct);
    }

    private static void RequireEditableTrip(Trip trip)
    {
        if (trip.Status is not (TripStatus.Draft or TripStatus.Planned) || trip.ActualLoadingAt != null
            || trip.ActualDepartureAt != null || trip.ActualArrivalAt != null)
            throw new ConflictException("Состав можно менять только у чернового или планового рейса до подтверждения и начала погрузки.");
    }
}
