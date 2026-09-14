using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class ProductListService(DispatcherDbContext db)
{
    private IQueryable<ProductListDto> Rows() =>
        from p in db.Products.AsNoTracking()
        let assignment = p.Assignments.Where(x => x.ValidTo == null)
            .OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.Id).FirstOrDefault()
        let takt = p.TaktAssignments.Where(x => x.ValidTo == null)
            .OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.Id).FirstOrDefault()
        let demand = p.Demands.Where(x => x.Status != DemandStatus.Cancelled && x.Status != DemandStatus.Fulfilled && x.CurrentRevisionId != null)
            .OrderBy(x => x.CurrentRevision!.RequiredDeliveryDate).ThenBy(x => x.Id).FirstOrDefault()
        let production = p.Demands.Where(x => x.Status != DemandStatus.Cancelled)
            .SelectMany(x => x.Revisions.Where(r => r.Id == x.CurrentRevisionId))
            .SelectMany(x => x.ProductionAssignments)
            .Where(x => x.ValidTo == null && x.Status != ProductionAssignmentStatus.Cancelled)
            .OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.Id).FirstOrDefault()
        let storage = p.StoragePlacements.Where(x => x.DepartedAt == null)
            .OrderByDescending(x => x.ArrivedAt).ThenByDescending(x => x.Id).FirstOrDefault()
        let tripItem = p.TripItems.Where(x => x.Trip.Status != TripStatus.Cancelled && x.Trip.Status != TripStatus.Completed)
            .OrderByDescending(x => x.AddedAt).ThenByDescending(x => x.Id).FirstOrDefault()
        select new ProductListDto
        {
            Id = p.Id, ProductCode = p.ProductCode, ProductTypeId = p.ProductTypeId, ProductTypeCode = p.ProductType.Code, ProductTypeName = p.ProductType.Name,
            Mark = p.Mark, Status = p.Status, WeightKg = p.WeightKg, AdditionalInfo = p.AdditionalInfo,
            WidthMm = p.WidthMm, HeightMm = p.HeightMm, ThicknessMm = p.ThicknessMm,
            CorniceWidthIncreaseMm = p.CorniceWidthIncreaseMm, TotalWidthWithCorniceMm = p.TotalWidthWithCorniceMm,
            ThicknessIncreaseMm = p.ThicknessIncreaseMm, RightBendMm = p.RightBendMm, LeftBendMm = p.LeftBendMm,
            CladdingWidthWithBendsMm = p.CladdingWidthWithBendsMm, Version = p.Version, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt,
            ProjectObjectId = p.ProjectPosition == null ? null : p.ProjectPosition.ConstructionObjectId,
            ProjectObjectName = p.ProjectPosition == null ? null : p.ProjectPosition.ConstructionObject.Name,
            ConstructionObjectId = assignment != null ? assignment.ConstructionObjectId : p.ProjectPosition == null ? null : p.ProjectPosition.ConstructionObjectId,
            ConstructionObjectCode = assignment != null ? assignment.ConstructionObject.Code : p.ProjectPosition == null ? null : p.ProjectPosition.ConstructionObject.Code,
            ConstructionObjectName = assignment != null ? assignment.ConstructionObject.Name : p.ProjectPosition == null ? null : p.ProjectPosition.ConstructionObject.Name,
            BuildingSectionId = assignment != null ? assignment.BuildingSectionId : p.ProjectPosition == null ? null : p.ProjectPosition.BuildingSectionId,
            BuildingSectionName = assignment != null ? (assignment.BuildingSection == null ? null : assignment.BuildingSection.Name)
                : p.ProjectPosition == null || p.ProjectPosition.BuildingSection == null ? null : p.ProjectPosition.BuildingSection.Name,
            FloorId = assignment != null ? assignment.FloorId : p.ProjectPosition == null ? null : p.ProjectPosition.FloorId,
            FloorName = assignment != null ? (assignment.Floor == null ? null : assignment.Floor.Name)
                : p.ProjectPosition == null || p.ProjectPosition.Floor == null ? null : p.ProjectPosition.Floor.Name,
            InstallationNumber = assignment != null ? assignment.InstallationNumber : p.ProjectPosition == null ? null : p.ProjectPosition.InstallationNumber,
            BindingSource = assignment != null ? "assignment" : p.ProjectPosition == null ? null : "project",
            ConstructionTaktId = takt == null ? null : takt.ConstructionTaktId,
            ConstructionTaktCode = takt == null ? null : takt.ConstructionTakt.Code,
            ConstructionTaktName = takt == null ? null : takt.ConstructionTakt.Name,
            DemandId = demand == null ? null : demand.Id,
            DemandRevisionId = demand == null ? null : demand.CurrentRevisionId,
            DemandTaktId = demand == null ? null : demand.CurrentRevision!.ConstructionTaktId,
            DemandStatus = demand == null ? null : demand.Status,
            RequiredDeliveryDate = demand == null ? null : demand.CurrentRevision!.RequiredDeliveryDate,
            EarliestDeliveryDate = demand == null ? null : demand.CurrentRevision!.EarliestDeliveryDate,
            RequiredProductionStartDate = demand == null ? null : demand.CurrentRevision!.RequiredProductionStartDate,
            RequiredProductionEndDate = demand == null ? null : demand.CurrentRevision!.RequiredProductionEndDate,
            PriorityLevel = demand == null ? null : demand.CurrentRevision!.PriorityLevel,
            PriorityOrder = demand == null ? null : demand.CurrentRevision!.PriorityOrder,
            OpenDemandCount = p.Demands.Count(x => x.Status != DemandStatus.Cancelled && x.Status != DemandStatus.Fulfilled),
            ProductionAssignmentId = production == null ? null : production.Id,
            ProductionDemandRevisionId = production == null ? null : production.DemandRevisionId,
            PlantId = production == null ? null : production.ProductionLine.PlantId,
            PlantName = production == null ? null : production.ProductionLine.Plant.Name,
            ProductionLineId = production == null ? null : production.ProductionLineId,
            ProductionLineName = production == null ? null : production.ProductionLine.Name,
            PlannedProductionDate = production == null ? null : production.PlannedProductionDate,
            ProductionStatus = production == null ? null : production.Status,
            ProductionMethod = production == null ? null : production.AssignmentMethod,
            StoragePlacementId = storage == null ? null : storage.Id,
            StorageAreaId = storage == null ? null : storage.StorageAreaId,
            StorageAreaName = storage == null ? null : storage.StorageArea.Name,
            StorageArrivedAt = storage == null ? null : storage.ArrivedAt,
            LastStorageDepartedAt = p.StoragePlacements.Where(x => x.DepartedAt != null)
                .OrderByDescending(x => x.DepartedAt).ThenByDescending(x => x.Id).Select(x => x.DepartedAt).FirstOrDefault(),
            TripId = tripItem == null ? null : tripItem.TripId,
            TripNumber = tripItem == null ? null : tripItem.Trip.TripNumber,
            TripStatus = tripItem == null ? null : tripItem.Trip.Status,
            ActiveTripCount = p.TripItems.Count(x => x.Trip.Status != TripStatus.Cancelled && x.Trip.Status != TripStatus.Completed),
            LoadingSequence = tripItem == null ? null : tripItem.LoadingSequence,
            TripPlantId = tripItem == null ? null : tripItem.Trip.PlantId,
            TripPlantName = tripItem == null ? null : tripItem.Trip.Plant.Name,
            TripObjectId = tripItem == null ? null : tripItem.Trip.ConstructionObjectId,
            TripObjectName = tripItem == null ? null : tripItem.Trip.ConstructionObject.Name,
            UnloadingPointId = tripItem == null ? null : tripItem.Trip.UnloadingPointId,
            UnloadingPointName = tripItem == null || tripItem.Trip.UnloadingPoint == null ? null : tripItem.Trip.UnloadingPoint.Name,
            VehicleId = tripItem == null ? null : tripItem.Trip.VehicleId,
            VehicleRegistrationNumber = tripItem == null ? null : tripItem.Trip.Vehicle.RegistrationNumber,
            VehicleMake = tripItem == null ? null : tripItem.Trip.Vehicle.Make,
            VehicleModel = tripItem == null ? null : tripItem.Trip.Vehicle.Model,
            CarrierId = tripItem == null ? null : tripItem.Trip.Vehicle.CarrierId,
            CarrierName = tripItem == null ? null : tripItem.Trip.Vehicle.Carrier.Name,
            PlannedLoadingAt = tripItem == null ? null : tripItem.Trip.PlannedLoadingAt,
            PlannedDepartureAt = tripItem == null ? null : tripItem.Trip.PlannedDepartureAt,
            PlannedArrivalAt = tripItem == null ? null : tripItem.Trip.PlannedArrivalAt,
            ActualLoadingAt = tripItem == null ? null : tripItem.Trip.ActualLoadingAt,
            ActualDepartureAt = tripItem == null ? null : tripItem.Trip.ActualDepartureAt,
            ActualArrivalAt = tripItem == null ? null : tripItem.Trip.ActualArrivalAt,
        };

    public async Task<PagedResult<ProductListDto>> ListAsync(ProductListQuery request, CancellationToken ct)
    {
        var query = ProductListFilters.Apply(Rows(), request);
        // Identifier predicates are grouped so type/value/active constraints match the SAME identifier.
        var identifierFilters = request.Filters.Where(x => x.Field.StartsWith("identifiers.", StringComparison.OrdinalIgnoreCase)).ToList();
        if (identifierFilters.Count > 0)
        {
            var matching = ProductListFilters.FilterIdentifiers(db.ProductIdentifiers.AsNoTracking(), identifierFilters);
            query = query.Where(x => matching.Any(i => i.ProductId == x.Id));
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(x => x.ProductCode.ToUpper().Contains(term) || x.Mark.ToUpper().Contains(term)
                || (x.InstallationNumber != null && x.InstallationNumber.ToUpper().Contains(term))
                || (x.AdditionalInfo != null && x.AdditionalInfo.ToUpper().Contains(term))
                || db.ProductIdentifiers.Any(i => i.ProductId == x.Id && i.Value.ToUpper().Contains(term)));
        }
        var total = await query.CountAsync(ct);
        var items = await ProductListFilters.Sort(query, request.SortBy, request.Descending)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
        // Only load identifiers for this page; avoid an N+1 query or multiplying product rows.
        var ids = items.Select(x => x.Id).ToArray();
        var identifiers = await db.ProductIdentifiers.AsNoTracking().Where(x => ids.Contains(x.ProductId))
            .OrderBy(x => x.Id).ToListAsync(ct);
        var byProduct = identifiers.ToLookup(x => x.ProductId);
        foreach (var item in items)
            item.Identifiers = byProduct[item.Id].Select(x => new ProductListIdentifierDto
            { Id = x.Id, Type = x.Type, Value = x.Value, IsActive = x.IsActive, AssignedAt = x.AssignedAt, RevokedAt = x.RevokedAt }).ToList();
        return new() { Items = items, Total = total, Page = request.Page, PageSize = request.PageSize };
    }
}
