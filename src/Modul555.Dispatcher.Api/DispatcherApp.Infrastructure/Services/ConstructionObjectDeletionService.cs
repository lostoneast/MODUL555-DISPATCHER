using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DispatcherApp.Application.Auth;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class ConstructionObjectDeletionService(DispatcherDbContext db, ICurrentUser user)
{
    // No cascade through shared plants, carriers, vehicles or product types.
    private sealed class Plan
    {
        public required string Name { get; init; }
        public int[] Sections { get; set; } = [];
        public int[] Floors { get; set; } = [];
        public long[] Takts { get; set; } = [];
        public int[] StorageAreas { get; set; } = [];
        public int[] UnloadingPoints { get; set; } = [];
        public int[] Routes { get; set; } = [];
        public long[] Trips { get; set; } = [];
        public long[] TripItems { get; set; } = [];
        public long[] Placements { get; set; } = [];
        public long[] Products { get; set; } = [];
        public long[] PhysicalProducts { get; set; } = [];
        public long[] Positions { get; set; } = [];
        public long[] Assignments { get; set; } = [];
        public long[] TaktAssignments { get; set; } = [];
        public long[] Identifiers { get; set; } = [];
        public long[] Revisions { get; set; } = [];
        public long[] Demands { get; set; } = [];
        public long[] ProductionAssignments { get; set; } = [];
        public long[] ResetDemands { get; set; } = [];
        public List<ProductStamp> RelatedProducts { get; set; } = [];
        public IReadOnlyDictionary<string, int> Counts() => new Dictionary<string, int>
        {
            ["Объекты"] = 1, ["Секции"] = Sections.Length, ["Этажи"] = Floors.Length,
            ["Такты"] = Takts.Length, ["Склады объекта"] = StorageAreas.Length,
            ["Точки разгрузки"] = UnloadingPoints.Length, ["Маршруты"] = Routes.Length,
            ["Рейсы"] = Trips.Length, ["Позиции рейсов"] = TripItems.Length,
            ["Размещения на складе"] = Placements.Length, ["Изделия"] = Products.Length,
            ["Проектные привязки"] = Positions.Length, ["Назначения изделий"] = Assignments.Length,
            ["Назначения на такты"] = TaktAssignments.Length, ["Идентификаторы"] = Identifiers.Length,
            ["Версии потребностей"] = Revisions.Length, ["Потребности"] = Demands.Length,
            ["Производственные назначения"] = ProductionAssignments.Length,
        };
        [System.Text.Json.Serialization.JsonIgnore]
        public string Token => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this))));
    }

    private sealed record ProductStamp(long Id, long Version, ProductStatus Status);

    private async Task<Plan> BuildPlan(int id, CancellationToken ct)
    {
        var obj = await db.ConstructionObjects.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException();
        var p = new Plan { Name = obj.Name };
        p.Sections = await db.BuildingSections.Where(x => x.ConstructionObjectId == id).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Floors = await db.Floors.Where(x => p.Sections.Contains(x.BuildingSectionId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Takts = await db.ConstructionTakts.Where(x => x.ConstructionObjectId == id).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.StorageAreas = await db.StorageAreas.Where(x => x.ConstructionObjectId == id).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.UnloadingPoints = await db.UnloadingPoints.Where(x => x.ConstructionObjectId == id).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Routes = await db.TransportRoutes.Where(x => x.ConstructionObjectId == id).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Trips = await db.Trips.Where(x => x.ConstructionObjectId == id).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);

        var related = db.Products.Where(x =>
            (x.ProjectPosition != null && x.ProjectPosition.ConstructionObjectId == id)
            || x.Assignments.Any(a => a.ConstructionObjectId == id)
            || x.TaktAssignments.Any(a => p.Takts.Contains(a.ConstructionTaktId))
            || x.Demands.Any(d => d.Revisions.Any(r => p.Takts.Contains(r.ConstructionTaktId)))
            || x.StoragePlacements.Any(s => p.StorageAreas.Contains(s.StorageAreaId))
            || x.TripItems.Any(t => p.Trips.Contains(t.TripId)));
        p.RelatedProducts = await related.OrderBy(x => x.Id).Select(x => new ProductStamp(x.Id, x.Version, x.Status)).ToListAsync(ct);

        // Only unproduced virtual products exclusively belonging to this object may disappear.
        // Cancelled is deliberately not sufficient evidence that a physical product never existed.
        p.PhysicalProducts = await related.Where(x => x.Status != ProductStatus.Created
            || x.StoragePlacements.Any() || x.TripItems.Any() || x.Identifiers.Any()
            || x.Demands.Any(d => d.Status == DemandStatus.Fulfilled
                || d.Revisions.Any(r => r.ProductionAssignments.Any(a => a.Status >= ProductionAssignmentStatus.InProduction)))
            || db.AuditEvents.Any(a => a.EntityType == nameof(Product) && a.EntityId == x.Id.ToString() && a.Action == "PreservePhysicalProduct"))
            .OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Products = await related.Where(x => !p.PhysicalProducts.Contains(x.Id)
            && (x.ProjectPosition == null || x.ProjectPosition.ConstructionObjectId == id)
            && !x.Assignments.Any(a => a.ConstructionObjectId != id)
            && !x.TaktAssignments.Any(a => !p.Takts.Contains(a.ConstructionTaktId))
            && !x.Demands.Any(d => d.Revisions.Any(r => !p.Takts.Contains(r.ConstructionTaktId))))
            .OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);

        p.Positions = await db.ProjectPositions.Where(x => x.ConstructionObjectId == id || p.Products.Contains(x.ProductId)).OrderBy(x => x.ProductId).Select(x => x.ProductId).ToArrayAsync(ct);
        p.Assignments = await db.ProductAssignments.Where(x => x.ConstructionObjectId == id || p.Products.Contains(x.ProductId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.TaktAssignments = await db.TaktAssignments.Where(x => p.Takts.Contains(x.ConstructionTaktId) || p.Products.Contains(x.ProductId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.TripItems = await db.TripItems.Where(x => p.Trips.Contains(x.TripId) || p.Products.Contains(x.ProductId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Placements = await db.StoragePlacements.Where(x => p.StorageAreas.Contains(x.StorageAreaId) || p.Products.Contains(x.ProductId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Identifiers = await db.ProductIdentifiers.Where(x => p.Products.Contains(x.ProductId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Revisions = await db.DemandRevisions.Where(x => p.Takts.Contains(x.ConstructionTaktId) || p.Products.Contains(x.Demand.ProductId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.Demands = await db.Demands.Where(x => p.Products.Contains(x.ProductId)
            || (x.Revisions.Any(r => p.Revisions.Contains(r.Id)) && !x.Revisions.Any(r => !p.Revisions.Contains(r.Id))))
            .OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.ResetDemands = await db.Demands.Where(x => x.CurrentRevisionId.HasValue && p.Revisions.Contains(x.CurrentRevisionId.Value))
            .OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        p.ProductionAssignments = await db.ProductionAssignments.Where(x => p.Revisions.Contains(x.DemandRevisionId)).OrderBy(x => x.Id).Select(x => x.Id).ToArrayAsync(ct);
        return p;
    }

    private static ObjectDeletionPreview Preview(Plan p) => new(p.Name, p.Token,
        p.Products.Length, p.RelatedProducts.Count - p.Products.Length, p.Counts());

    public async Task<ObjectDeletionPreview> PreviewAsync(int id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var result = Preview(await BuildPlan(id, ct));
        await tx.CommitAsync(ct);
        return result;
    }

    public async Task DeleteAsync(int id, ObjectDeletionRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var p = await BuildPlan(id, ct);
        if (!string.Equals(request.ConfirmationName, p.Name, StringComparison.Ordinal))
            throw new ArgumentException("Введите точное название объекта с учётом регистра и пробелов.");
        if (!string.Equals(request.PreviewToken, p.Token, StringComparison.Ordinal))
            throw new ConflictException("Состав связанных данных изменился. Обновите предупреждение и подтвердите удаление заново.");

        // Break the Demand <-> DemandRevision cycle before removing revisions.
        await db.Demands.Where(x => p.ResetDemands.Contains(x.Id)).ExecuteUpdateAsync(s => s
            .SetProperty(x => x.CurrentRevisionId, (long?)null)
            .SetProperty(x => x.Status, DemandStatus.Draft)
            .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow)
            .SetProperty(x => x.Version, x => x.Version + 1), ct);
        await db.ProductionAssignments.Where(x => p.ProductionAssignments.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.DemandRevisions.Where(x => p.Revisions.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.Demands.Where(x => p.Demands.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.TripItems.Where(x => p.TripItems.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.Trips.Where(x => p.Trips.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.StoragePlacements.Where(x => p.Placements.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.TaktAssignments.Where(x => p.TaktAssignments.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.ProductAssignments.Where(x => p.Assignments.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.ProjectPositions.Where(x => p.Positions.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
        await db.ProductIdentifiers.Where(x => p.Identifiers.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.Products.Where(x => p.Products.Contains(x.Id)).ExecuteDeleteAsync(ct);
        var retained = p.RelatedProducts.Select(x => x.Id).Except(p.Products).ToArray();
        await db.Products.Where(x => retained.Contains(x.Id)).ExecuteUpdateAsync(s => s
            .SetProperty(x => x.Version, x => x.Version + 1).SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), ct);
        await db.ConstructionTakts.Where(x => p.Takts.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.Floors.Where(x => p.Floors.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.BuildingSections.Where(x => p.Sections.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.UnloadingPoints.Where(x => p.UnloadingPoints.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.TransportRoutes.Where(x => p.Routes.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.StorageAreas.Where(x => p.StorageAreas.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await db.ConstructionObjects.Where(x => x.Id == id).ExecuteDeleteAsync(ct);
        // Retain durable evidence even when the object's storage/production history is removed.
        // Otherwise a stale Created status could make a real product deletable at another object later.
        foreach (var productId in p.PhysicalProducts)
            db.AuditEvents.Add(new AuditEvent
            {
                Timestamp = DateTimeOffset.UtcNow, UserId = user.UserId, UserName = user.UserName,
                EntityType = nameof(Product), EntityId = productId.ToString(), Action = "PreservePhysicalProduct",
                Comment = $"Изделие сохранено при удалении объекта {id}: присутствуют признаки физического изделия или спорного состояния.",
            });
        db.AuditEvents.Add(new AuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow, UserId = user.UserId, UserName = user.UserName,
            EntityType = nameof(ConstructionObject), EntityId = id.ToString(), Action = "Delete",
            OldValuesJson = JsonSerializer.Serialize(Preview(p)),
            Comment = "Удаление подтверждено точным названием. Физические и спорные изделия сохранены.",
        });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
