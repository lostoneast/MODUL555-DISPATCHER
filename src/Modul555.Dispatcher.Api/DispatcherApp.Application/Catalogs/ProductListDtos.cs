using System.ComponentModel.DataAnnotations;
using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Application.Catalogs;

public sealed class ProductListQuery
{
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 200)] public int PageSize { get; set; } = 20;
    [MaxLength(256)] public string? Search { get; set; }
    public string SortBy { get; set; } = "productCode";
    public bool Descending { get; set; }
    [MaxLength(50)] public List<ProductFieldFilter> Filters { get; set; } = [];
}

public enum ProductFilterOperator { Eq, NotEq, Contains, Gte, Lte, IsEmpty, IsNotEmpty }

public sealed class ProductFieldFilter
{
    [Required] public string Field { get; set; } = "";
    [EnumDataType(typeof(ProductFilterOperator))]
    public ProductFilterOperator Operator { get; set; } = ProductFilterOperator.Eq;
    [MaxLength(2000)] public string? Value { get; set; }
}

public sealed record ProductFieldInfo(string Field, string Type, bool Nullable, bool Sortable,
    IReadOnlyList<string> Operators, IReadOnlyList<string> Values);

// One row per product. Collections and historical records will be shown in the detail page.
public sealed class ProductListDto : ProductDto
{
    public string ProductTypeCode { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int? ProjectObjectId { get; set; }
    public string? ProjectObjectName { get; set; }
    public int? ConstructionObjectId { get; set; }
    public string? ConstructionObjectCode { get; set; }
    public string? ConstructionObjectName { get; set; }
    public int? BuildingSectionId { get; set; }
    public string? BuildingSectionName { get; set; }
    public int? FloorId { get; set; }
    public string? FloorName { get; set; }
    public string? InstallationNumber { get; set; }
    public string? BindingSource { get; set; }
    public long? ConstructionTaktId { get; set; }
    public string? ConstructionTaktCode { get; set; }
    public string? ConstructionTaktName { get; set; }
    public long? DemandId { get; set; }
    public long? DemandRevisionId { get; set; }
    public long? DemandTaktId { get; set; }
    public DemandStatus? DemandStatus { get; set; }
    public DateOnly? RequiredDeliveryDate { get; set; }
    public DateOnly? EarliestDeliveryDate { get; set; }
    public DateOnly? RequiredProductionStartDate { get; set; }
    public DateOnly? RequiredProductionEndDate { get; set; }
    public PriorityLevel? PriorityLevel { get; set; }
    public int? PriorityOrder { get; set; }
    public int OpenDemandCount { get; set; }
    public long? ProductionAssignmentId { get; set; }
    public long? ProductionDemandRevisionId { get; set; }
    public int? PlantId { get; set; }
    public string? PlantName { get; set; }
    public int? ProductionLineId { get; set; }
    public string? ProductionLineName { get; set; }
    public DateOnly? PlannedProductionDate { get; set; }
    public ProductionAssignmentStatus? ProductionStatus { get; set; }
    public ProductionAssignmentMethod? ProductionMethod { get; set; }
    public long? StoragePlacementId { get; set; }
    public int? StorageAreaId { get; set; }
    public string? StorageAreaName { get; set; }
    public DateTimeOffset? StorageArrivedAt { get; set; }
    public DateTimeOffset? LastStorageDepartedAt { get; set; }
    public long? TripId { get; set; }
    public string? TripNumber { get; set; }
    public TripStatus? TripStatus { get; set; }
    public int ActiveTripCount { get; set; }
    public int? LoadingSequence { get; set; }
    public int? TripPlantId { get; set; }
    public string? TripPlantName { get; set; }
    public int? TripObjectId { get; set; }
    public string? TripObjectName { get; set; }
    public int? UnloadingPointId { get; set; }
    public string? UnloadingPointName { get; set; }
    public long? VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public int? CarrierId { get; set; }
    public string? CarrierName { get; set; }
    public DateTimeOffset? PlannedLoadingAt { get; set; }
    public DateTimeOffset? PlannedDepartureAt { get; set; }
    public DateTimeOffset? PlannedArrivalAt { get; set; }
    public DateTimeOffset? ActualLoadingAt { get; set; }
    public DateTimeOffset? ActualDepartureAt { get; set; }
    public DateTimeOffset? ActualArrivalAt { get; set; }
    public List<ProductListIdentifierDto> Identifiers { get; set; } = [];
}

public sealed class ProductListIdentifierDto
{
    public long Id { get; set; }
    public ProductIdentifierType Type { get; set; }
    public string Value { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
