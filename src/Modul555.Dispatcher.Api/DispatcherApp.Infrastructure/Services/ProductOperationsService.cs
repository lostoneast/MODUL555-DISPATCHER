using System.Data;
using System.Globalization;
using System.Text.Json;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class ProductOperationsService(DispatcherDbContext db)
{
    private async Task<Product> RequireProduct(long id, ProductOperationDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Length > 512)
            throw new ArgumentException("Укажите причину изменения (до 512 символов).");
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException();
        if (dto.ProductVersion != product.Version) throw new ConflictException("Изделие изменено другим пользователем. Обновите карточку и повторите операцию.");
        db.Entry(product).Property(x => x.Version).OriginalValue = dto.ProductVersion;
        product.Version++;
        product.UpdatedAt = DateTimeOffset.UtcNow;
        return product;
    }

    private void Audit(long productId, string action, string reason, string? userId, object? before, object? after)
        => db.AuditEvents.Add(new AuditEvent { EntityType = nameof(Product), EntityId = productId.ToString(CultureInfo.InvariantCulture),
            Action = action, Comment = reason.Trim(), UserId = userId, Timestamp = DateTimeOffset.UtcNow, Source = AuditSource.UI,
            OldValuesJson = before == null ? null : JsonSerializer.Serialize(before), NewValuesJson = after == null ? null : JsonSerializer.Serialize(after) });

    public async Task SaveIdentifierAsync(long productId, long? identifierId, ProductIdentifierWriteDto dto, string? userId, CancellationToken ct)
    {
        if (!Enum.IsDefined(dto.Type) || string.IsNullOrWhiteSpace(dto.Value) || dto.Value.Length > 256)
            throw new ArgumentException("Выберите тип и укажите значение идентификатора (до 256 символов).");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await RequireProduct(productId, dto, ct);
        ProductIdentifier? previous = null;
        if (identifierId.HasValue)
        {
            previous = await db.ProductIdentifiers.SingleOrDefaultAsync(x => x.Id == identifierId && x.ProductId == productId, ct)
                ?? throw new KeyNotFoundException();
            if (!previous.IsActive) throw new ConflictException("Отозванный идентификатор нельзя изменить.");
            if (previous.Type == dto.Type && previous.Value == dto.Value.Trim()) throw new ArgumentException("Тип и значение идентификатора не изменились.");
        }
        var value = dto.Value.Trim();
        if (await db.ProductIdentifiers.AnyAsync(x => x.Type == dto.Type && x.Value == value, ct))
            throw new ConflictException("Такой тип и значение уже зарегистрированы. Отозванные идентификаторы также нельзя использовать повторно.");
        var now = DateTimeOffset.UtcNow;
        if (previous != null) { previous.IsActive = false; previous.RevokedAt = now; }
        var identifier = new ProductIdentifier { ProductId = productId, Type = dto.Type, Value = value, AssignedAt = now, IsActive = true };
        db.ProductIdentifiers.Add(identifier);
        Audit(productId, "EditProductIdentifier", dto.Reason, userId,
            previous == null ? null : new { previous.Id, previous.Type, previous.Value }, new { identifier.Type, identifier.Value });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task RevokeIdentifierAsync(long productId, long identifierId, ProductOperationDto dto, string? userId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await RequireProduct(productId, dto, ct);
        var identifier = await db.ProductIdentifiers.SingleOrDefaultAsync(x => x.Id == identifierId && x.ProductId == productId, ct)
            ?? throw new KeyNotFoundException();
        if (!identifier.IsActive) throw new ConflictException("Идентификатор уже отозван.");
        identifier.IsActive = false; identifier.RevokedAt = DateTimeOffset.UtcNow;
        Audit(productId, "RevokeProductIdentifier", dto.Reason, userId, new { identifier.Id, identifier.Type, identifier.Value }, null);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task<int?> ObjectId(long productId, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().Where(x => x.Id == productId).Select(x => new {
            AssignmentObject = x.Assignments.Where(a => a.ValidTo == null).OrderByDescending(a => a.ValidFrom).ThenByDescending(a => a.Id)
                .Select(a => (int?)a.ConstructionObjectId).FirstOrDefault(),
            ProjectObject = x.ProjectPosition == null ? (int?)null : x.ProjectPosition.ConstructionObjectId }).SingleOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException();
        return product.AssignmentObject ?? product.ProjectObject;
    }

    public async Task<IReadOnlyList<ProductDemandTaktOption>> TaktsAsync(long productId, CancellationToken ct)
    {
        var objectId = await ObjectId(productId, ct);
        return await db.ConstructionTakts.AsNoTracking().Where(x => x.ConstructionObjectId == objectId && x.ConstructionObject.IsActive
                && x.Status != ConstructionTaktStatus.Completed && x.Status != ConstructionTaktStatus.Cancelled)
            .OrderBy(x => x.Sequence).ThenBy(x => x.Id)
            .Select(x => new ProductDemandTaktOption(x.Id, x.Name, x.PlannedProductionStartDate, x.PlannedProductionEndDate)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductDemandDto>> DemandsAsync(long productId, CancellationToken ct)
    {
        if (!await db.Products.AnyAsync(x => x.Id == productId, ct)) throw new KeyNotFoundException();
        var demands = await db.Demands.AsNoTracking().Where(x => x.ProductId == productId).OrderByDescending(x => x.Id).ToListAsync(ct);
        var revisions = await db.DemandRevisions.AsNoTracking().Where(x => x.Demand.ProductId == productId)
            .OrderByDescending(x => x.RevisionNumber).Select(x => new { x.DemandId,
                Revision = new ProductDemandRevisionDto(x.Id, x.RevisionNumber, x.ConstructionTaktId, x.ConstructionTakt.Name,
                    x.RequiredProductionStartDate, x.RequiredProductionEndDate, x.EarliestDeliveryDate, x.RequiredDeliveryDate,
                    x.PriorityLevel, x.PriorityOrder, x.ChangeReason, x.Comment, x.CreatedAt, x.CreatedByUserId) }).ToListAsync(ct);
        var assigned = await db.ProductionAssignments.AsNoTracking().Where(x => x.DemandRevision.Demand.ProductId == productId
            && x.ValidTo == null && x.Status != ProductionAssignmentStatus.Cancelled).Select(x => x.DemandRevision.DemandId).Distinct().ToListAsync(ct);
        var grouped = revisions.ToLookup(x => x.DemandId, x => x.Revision);
        return demands.Select(x => new ProductDemandDto(x.Id, x.Version, x.Status, x.CurrentRevisionId, assigned.Contains(x.Id), grouped[x.Id].ToList())).ToList();
    }

    private async Task RequireEditableDemand(Demand demand, long? version, CancellationToken ct)
    {
        if (version != demand.Version) throw new ConflictException("Потребность изменена. Обновите данные перед сохранением.");
        if (demand.Status is DemandStatus.Fulfilled or DemandStatus.Cancelled)
            throw new ConflictException("Выполненную или отменённую потребность нельзя изменить.");
        if (await db.ProductionAssignments.AnyAsync(x => x.DemandRevision.DemandId == demand.Id
            && x.ValidTo == null && x.Status != ProductionAssignmentStatus.Cancelled, ct))
            throw new ConflictException("У потребности есть действующее производственное назначение. Сначала пересмотрите производственный план.");
    }

    public async Task SaveDemandAsync(long productId, long? demandId, ProductDemandWriteDto dto, string? userId, CancellationToken ct)
    {
        if (dto.Status is not (DemandStatus.Draft or DemandStatus.Active) || !Enum.IsDefined(dto.PriorityLevel) || dto.PriorityOrder < 0)
            throw new ArgumentException("Допустимы статусы «Черновик» и «Активна», известный приоритет и неотрицательный порядок.");
        if (dto.EarliestDeliveryDate == default || dto.RequiredDeliveryDate == default || dto.EarliestDeliveryDate > dto.RequiredDeliveryDate)
            throw new ArgumentException("Укажите корректный диапазон доставки: ранняя дата не должна быть позже даты потребности.");
        if (dto.Comment?.Length > 2000) throw new ArgumentException("Примечание ограничено 2000 символами.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var product = await RequireProduct(productId, dto, ct);
        if (product.Status is ProductStatus.Delivered or ProductStatus.Cancelled)
            throw new ConflictException("Для доставленного или отменённого изделия нельзя создавать или пересматривать потребность.");
        var objectId = await ObjectId(productId, ct);
        var takt = await db.ConstructionTakts.SingleOrDefaultAsync(x => x.Id == dto.ConstructionTaktId && x.ConstructionObjectId == objectId
            && x.ConstructionObject.IsActive && x.Status != ConstructionTaktStatus.Completed && x.Status != ConstructionTaktStatus.Cancelled, ct)
            ?? throw new ArgumentException("Выберите незавершённый такт текущего активного объекта изделия.");
        if (takt.PlannedProductionStartDate > takt.PlannedProductionEndDate || takt.PlannedProductionEndDate > dto.RequiredDeliveryDate)
            throw new ArgumentException("Проверьте производственное окно такта: его окончание не должно быть позже даты потребности.");
        if (await db.Demands.AnyAsync(x => x.ProductId == productId && x.Id != demandId && x.Status != DemandStatus.Cancelled
            && x.Status != DemandStatus.Fulfilled && x.CurrentRevision != null && x.CurrentRevision.ConstructionTaktId == dto.ConstructionTaktId, ct))
            throw new ConflictException("Для изделия уже есть открытая потребность в этом такте. Измените существующую.");
        Demand demand;
        var now = DateTimeOffset.UtcNow;
        long? previousRevision = null;
        if (demandId.HasValue)
        {
            demand = await db.Demands.SingleOrDefaultAsync(x => x.Id == demandId && x.ProductId == productId, ct) ?? throw new KeyNotFoundException();
            await RequireEditableDemand(demand, dto.DemandVersion, ct);
            previousRevision = demand.CurrentRevisionId;
            demand.Version++;
        }
        else
        {
            demand = new Demand { ProductId = productId, CreatedAt = now, UpdatedAt = now, Version = 1, Status = dto.Status };
            db.Demands.Add(demand);
            await db.SaveChangesAsync(ct);
        }
        var number = (await db.DemandRevisions.Where(x => x.DemandId == demand.Id).Select(x => (int?)x.RevisionNumber).MaxAsync(ct) ?? 0) + 1;
        var revision = new DemandRevision { DemandId = demand.Id, RevisionNumber = number, ConstructionTaktId = takt.Id,
            RequiredProductionStartDate = takt.PlannedProductionStartDate, RequiredProductionEndDate = takt.PlannedProductionEndDate,
            EarliestDeliveryDate = dto.EarliestDeliveryDate, RequiredDeliveryDate = dto.RequiredDeliveryDate,
            PriorityLevel = dto.PriorityLevel, PriorityOrder = dto.PriorityOrder, ChangeReason = dto.Reason.Trim(),
            Comment = dto.Comment?.Trim(), CreatedAt = now, CreatedByUserId = userId };
        db.DemandRevisions.Add(revision);
        await db.SaveChangesAsync(ct);
        demand.CurrentRevisionId = revision.Id;
        demand.Status = dto.Status; demand.UpdatedAt = now;
        Audit(productId, "ReviseProductDemand", dto.Reason, userId, new { DemandId = demand.Id, CurrentRevisionId = previousRevision },
            new { DemandId = demand.Id, CurrentRevisionId = revision.Id, dto.Status, dto.ConstructionTaktId, dto.EarliestDeliveryDate,
                dto.RequiredDeliveryDate, dto.PriorityLevel, dto.PriorityOrder });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task CancelDemandAsync(long productId, long demandId, ProductDemandCancelDto dto, string? userId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await RequireProduct(productId, dto, ct);
        var demand = await db.Demands.SingleOrDefaultAsync(x => x.Id == demandId && x.ProductId == productId, ct) ?? throw new KeyNotFoundException();
        await RequireEditableDemand(demand, dto.DemandVersion, ct);
        var previousStatus = demand.Status;
        demand.Status = DemandStatus.Cancelled; demand.Version++; demand.UpdatedAt = DateTimeOffset.UtcNow;
        Audit(productId, "CancelProductDemand", dto.Reason, userId, new { demand.Id, Status = previousStatus }, new { demand.Id, demand.Status });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
