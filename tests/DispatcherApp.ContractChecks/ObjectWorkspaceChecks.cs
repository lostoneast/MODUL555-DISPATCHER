using System.Data.Common;
using DispatcherApp.Application.Auth;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using DispatcherApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

internal static class ObjectWorkspaceChecks
{
    // Deliberately fixed to the disposable local test container, never production configuration.
    private const string Connection = "Host=127.0.0.1;Port=55439;Database=object_workspace_tests;Username=workspace_test;Password=workspace_test_only";
    private sealed class TestUser : ICurrentUser
    {
        public string UserId => "contract-test";
        public string UserName => "contract-test";
        public IReadOnlyList<string> Roles => ["manager"];
    }
    private sealed class FailDelete : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase) && command.CommandText.Contains("floors"))
                throw new InvalidOperationException("Injected deletion failure");
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
    private static DispatcherDbContext Open(bool fail = false)
    {
        var options = new DbContextOptionsBuilder<DispatcherDbContext>().UseNpgsql(Connection);
        if (fail) options.AddInterceptors(new FailDelete());
        return new(options.Options);
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static async Task Reject<T>(Func<Task> action) where T : Exception
    {
        try { await action(); } catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}");
    }
    private static ObjectProductWriteDto Write(ObjectProductDto row) => new()
    {
        ProductCode = row.ProductCode, ProductTypeId = row.ProductTypeId, Mark = row.Mark,
        WidthMm = row.WidthMm, HeightMm = row.HeightMm, ThicknessMm = row.ThicknessMm,
        CorniceWidthIncreaseMm = row.CorniceWidthIncreaseMm, TotalWidthWithCorniceMm = row.TotalWidthWithCorniceMm,
        ThicknessIncreaseMm = row.ThicknessIncreaseMm, RightBendMm = row.RightBendMm, LeftBendMm = row.LeftBendMm,
        CladdingWidthWithBendsMm = row.CladdingWidthWithBendsMm, WeightKg = row.WeightKg,
        Status = row.Status, Version = row.Version, BuildingSectionId = row.BuildingSectionId,
        FloorId = row.FloorId, InstallationNumber = row.InstallationNumber,
    };

    public static async Task RunAsync()
    {
        int id, otherId;
        long virtualId, physicalId, transferredId, createdId;
        await using (var db = Open())
        {
            await db.Database.EnsureCreatedAsync();
            Check(!await db.ConstructionObjects.AnyAsync(), "Integration tests require a fresh disposable database");
            await new DemoDataService(db).SeedAsync(default);
            id = await db.ConstructionObjects.Select(x => x.Id).SingleAsync();
            var other = new ConstructionObject { Code = "OTHER", Name = "Другой объект", Address = "Другой адрес" };
            db.ConstructionObjects.Add(other); await db.SaveChangesAsync(); otherId = other.Id;
            var typeId = await db.ProductTypes.Select(x => x.Id).FirstAsync();
            var section = await db.BuildingSections.SingleAsync();
            var floor = await db.Floors.FirstAsync();
            virtualId = await db.Products.Where(x => !x.Identifiers.Any()).Select(x => x.Id).SingleAsync();
            var physical = new Product { ProductCode = "PHYSICAL", ProductTypeId = typeId, Mark = "Физическое", WidthMm = 100, HeightMm = 100, ThicknessMm = 10, Version = 1, Status = ProductStatus.InStorage };
            var transferred = new Product { ProductCode = "TRANSFERRED", ProductTypeId = typeId, Mark = "Переназначено", WidthMm = 100, HeightMm = 100, ThicknessMm = 10, Version = 1 };
            db.Products.AddRange(physical, transferred); await db.SaveChangesAsync();
            physicalId = physical.Id; transferredId = transferred.Id;
            db.ProjectPositions.AddRange(new ProjectPosition { ProductId = physicalId, ConstructionObjectId = id }, new ProjectPosition { ProductId = transferredId, ConstructionObjectId = id });
            db.ProductAssignments.Add(new ProductAssignment { ProductId = transferredId, ConstructionObjectId = otherId, ValidFrom = DateTimeOffset.UtcNow });
            db.StoragePlacements.Add(new StoragePlacement { ProductId = physicalId, StorageAreaId = await db.StorageAreas.Where(x => x.ConstructionObjectId == id).Select(x => x.Id).SingleAsync(), ArrivedAt = DateTimeOffset.UtcNow });
            var trip = new Trip { TripNumber = "TEST-TRIP", ConstructionObjectId = id, PlantId = await db.Plants.Select(x => x.Id).SingleAsync(), VehicleId = await db.Vehicles.Select(x => x.Id).SingleAsync(), Version = 1 };
            trip.Items.Add(new TripItem { ProductId = physicalId }); db.Trips.Add(trip);
            var demand = new Demand { ProductId = virtualId, Version = 1 };
            var revision = new DemandRevision { Demand = demand, ConstructionTaktId = await db.ConstructionTakts.Select(x => x.Id).SingleAsync(), RevisionNumber = 1 };
            db.DemandRevisions.Add(revision); await db.SaveChangesAsync();
            demand.CurrentRevisionId = revision.Id;
            db.ProductionAssignments.Add(new ProductionAssignment { DemandRevisionId = revision.Id, ProductionLineId = await db.ProductionLines.Select(x => x.Id).FirstAsync(), Status = ProductionAssignmentStatus.Proposed });
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();

            var service = new ObjectProductsService(db, new CatalogService(db));
            var list = await service.ListAsync(id, new(), default);
            Check(list.Items.Any(x => x.Id == physicalId) && !list.Items.Any(x => x.Id == transferredId), "Effective object binding is wrong");
            foreach (var sort in new[] { "productCode", "mark", "productTypeName", "status", "weightKg", "updatedAt", "widthMm", "heightMm", "thicknessMm", "installationNumber" })
                await service.ListAsync(id, new() { SortBy = sort, Descending = true, PageSize = 1 }, default);
            var filtered = await service.ListAsync(id, new() { Search = "PHYSICAL", Status = ProductStatus.InStorage }, default);
            Check(filtered.Total == 1, "Server-side filters are wrong");
            var row = list.Items.Single(x => x.Id == virtualId);
            var edit = Write(row); edit.Mark = "Изменено";
            await service.UpdateAsync(id, virtualId, edit, default);
            await Reject<ConflictException>(() => service.UpdateAsync(id, virtualId, edit, default));
            edit.Version++;
            edit.FloorId = floor.Id; edit.BuildingSectionId = null;
            await Reject<ArgumentException>(() => service.UpdateAsync(id, virtualId, edit, default));
            edit.BuildingSectionId = section.Id; edit.InstallationNumber = "UPDATED";
            await service.UpdateAsync(id, virtualId, edit, default);
            Check(await db.ProductAssignments.AnyAsync(x => x.ProductId == virtualId && x.InstallationNumber == "UPDATED"), "Assignment history was not recorded");
            await Reject<KeyNotFoundException>(() => service.UpdateAsync(id, transferredId, edit, default));
            var newRow = Write(row); newRow.ProductCode = "NEW-PRODUCT";
            createdId = (await service.CreateAsync(id, newRow, default)).Id;
            Check(await db.ProjectPositions.AnyAsync(x => x.ProductId == createdId && x.ConstructionObjectId == id), "New product is not linked");
            var file = await service.ExportAsync(id, new() { PageSize = 1 }, default);
            using var workbook = new ClosedXML.Excel.XLWorkbook(new MemoryStream(file));
            Check(workbook.Worksheet(1).LastRowUsed()!.RowNumber() == 5, "Export incorrectly limited to one page");
        }
        ObjectDeletionPreview preview;
        await using (var db = Open())
        {
            var deletion = new ConstructionObjectDeletionService(db, new TestUser());
            preview = await deletion.PreviewAsync(id, default);
            Check(preview.ProductsToDelete == 2, "Virtual product deletion count is wrong");
            Check(preview.ProductsToKeep == 3, "Physical/shared product preservation count is wrong");
            await Reject<ArgumentException>(() => deletion.DeleteAsync(id, new() { ConfirmationName = preview.Name + " ", PreviewToken = preview.PreviewToken }, default));
            db.Floors.Add(new Floor { BuildingSectionId = await db.BuildingSections.Select(x => x.Id).SingleAsync(), Name = "Новый этаж", Number = 99 });
            await db.SaveChangesAsync();
            await Reject<ConflictException>(() => deletion.DeleteAsync(id, new() { ConfirmationName = preview.Name, PreviewToken = preview.PreviewToken }, default));
            preview = await deletion.PreviewAsync(id, default);
        }
        await using (var db = Open(true))
            await Reject<InvalidOperationException>(() => new ConstructionObjectDeletionService(db, new TestUser()).DeleteAsync(id,
                new() { ConfirmationName = preview.Name, PreviewToken = preview.PreviewToken }, default));
        await using (var db = Open())
        {
            Check(await db.Products.AnyAsync(x => x.Id == virtualId) && await db.Trips.AnyAsync(), "Failed delete did not roll back");
            var deletion = new ConstructionObjectDeletionService(db, new TestUser());
            await deletion.DeleteAsync(id, new() { ConfirmationName = preview.Name, PreviewToken = preview.PreviewToken }, default);
            Check(!await db.ConstructionObjects.AnyAsync(x => x.Id == id), "Object survived deletion");
            Check(!await db.Products.AnyAsync(x => x.Id == virtualId || x.Id == createdId), "Virtual products survived deletion");
            Check(await db.Products.AnyAsync(x => x.Id == physicalId) && await db.Products.AnyAsync(x => x.Id == transferredId), "Physical/shared products were deleted");
            Check(await db.ConstructionObjects.AnyAsync(x => x.Id == otherId) && await db.Plants.AnyAsync() && await db.Vehicles.AnyAsync(), "Shared entities were deleted");
            Check(!await db.ProjectPositions.AnyAsync(x => x.ConstructionObjectId == id) && !await db.Demands.AnyAsync(x => x.ProductId == virtualId), "Owned dependencies remain");
            Check(await db.ProductAssignments.AnyAsync(x => x.ProductId == transferredId && x.ConstructionObjectId == otherId), "Other object assignment was removed");
            Check(await db.AuditEvents.AnyAsync(x => x.Action == "Delete"), "Deletion audit is missing");
        }
        Console.WriteLine("PASS: PostgreSQL object filtering/sorting, edits, stale versions, link validation, creation, full export, deletion preview, stale confirmation, rollback, cascade and preservation.");
    }
}
