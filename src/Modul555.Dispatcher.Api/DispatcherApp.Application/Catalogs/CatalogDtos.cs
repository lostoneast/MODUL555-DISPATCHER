using System.ComponentModel.DataAnnotations;
using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Application.Catalogs;

public sealed class ProductTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProductTypeWriteDto
{
    [Required]
    public string Code { get; set; } = "";
    [Required]
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class PlantDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public bool IsActive { get; set; }
}

public sealed class PlantWriteDto
{
    [Required]
    public string Code { get; set; } = "";
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Address { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class ProductionLineDto
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public string? PlantName { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
}

public sealed class ProductionLineWriteDto
{
    public int PlantId { get; set; }
    [Required]
    public string Code { get; set; } = "";
    [Required]
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class LineCapabilityDto
{
    public int Id { get; set; }
    public int ProductionLineId { get; set; }
    public string? ProductionLineName { get; set; }
    public int ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }
    public int DefaultDailyCapacityUnits { get; set; }
    public bool IsActive { get; set; }
}

public sealed class LineCapabilityWriteDto
{
    public int ProductionLineId { get; set; }
    public int ProductTypeId { get; set; }
    public int DefaultDailyCapacityUnits { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ConstructionObjectDto
{
    public int SectionsCount { get; set; }
    public int FloorsCount { get; set; }
    public int TaktsCount { get; set; }
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class ConstructionObjectWriteDto
{
    [Required]
    public string Code { get; set; } = "";
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Address { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class BuildingSectionDto
{
    public int Id { get; set; }
    public int ConstructionObjectId { get; set; }
    public string? ConstructionObjectName { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class BuildingSectionWriteDto
{
    public int ConstructionObjectId { get; set; }
    [Required]
    public string Code { get; set; } = "";
    [Required]
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class FloorDto
{
    public int Id { get; set; }
    public int BuildingSectionId { get; set; }
    public string? BuildingSectionName { get; set; }
    public int? Number { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class FloorWriteDto
{
    public int BuildingSectionId { get; set; }
    public int? Number { get; set; }
    [Required]
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class UnloadingPointDto
{
    public int Id { get; set; }
    public int ConstructionObjectId { get; set; }
    public string? ConstructionObjectName { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UnloadingPointWriteDto
{
    public int ConstructionObjectId { get; set; }
    [Required]
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class StorageAreaDto
{
    public int Id { get; set; }
    public int? PlantId { get; set; }
    public string? PlantName { get; set; }
    public int? ConstructionObjectId { get; set; }
    public string? ConstructionObjectName { get; set; }
    public string Name { get; set; } = "";
    public int CapacityUnits { get; set; }
    public bool IsActive { get; set; }
}

public sealed class StorageAreaWriteDto
{
    public int? PlantId { get; set; }
    public int? ConstructionObjectId { get; set; }
    [Required]
    public string Name { get; set; } = "";
    public int CapacityUnits { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CarrierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CarrierWriteDto
{
    [Required]
    public string Name { get; set; } = "";
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class VehicleTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? RollingStockType { get; set; }
    public decimal? MinPayloadKg { get; set; }
    public decimal? MaxPayloadKg { get; set; }
    public int LoadingPlatformCount { get; set; }
    public decimal? LoadingPlatformLengthMm { get; set; }
    public decimal? LoadingPlatformWidthMm { get; set; }
    public decimal? AllowedRightLeftImbalanceKg { get; set; }
    public decimal? MaxCargoHeightMm { get; set; }
    public decimal? CargoVolumeM3 { get; set; }
    public decimal? TotalTrainLengthMm { get; set; }
    public decimal? TurningRadiusMm { get; set; }
    public string? Notes { get; set; }
}

public sealed class VehicleTypeWriteDto
{
    [Required]
    public string Name { get; set; } = "";
    public string? RollingStockType { get; set; }
    public decimal? MinPayloadKg { get; set; }
    public decimal? MaxPayloadKg { get; set; }
    public int LoadingPlatformCount { get; set; }
    public decimal? LoadingPlatformLengthMm { get; set; }
    public decimal? LoadingPlatformWidthMm { get; set; }
    public decimal? AllowedRightLeftImbalanceKg { get; set; }
    public decimal? MaxCargoHeightMm { get; set; }
    public decimal? CargoVolumeM3 { get; set; }
    public decimal? TotalTrainLengthMm { get; set; }
    public decimal? TurningRadiusMm { get; set; }
    public string? Notes { get; set; }
}

public sealed class VehicleDto
{
    public long Id { get; set; }
    public int VehicleTypeId { get; set; }
    public string? VehicleTypeName { get; set; }
    public int CarrierId { get; set; }
    public string? CarrierName { get; set; }
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public sealed class VehicleWriteDto
{
    public int VehicleTypeId { get; set; }
    public int CarrierId { get; set; }
    [Required]
    public string Make { get; set; } = "";
    [Required]
    public string Model { get; set; } = "";
    [Required]
    public string RegistrationNumber { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public sealed class TransportRouteDto
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public string? PlantName { get; set; }
    public int ConstructionObjectId { get; set; }
    public string? ConstructionObjectName { get; set; }
    public decimal? DistanceKm { get; set; }
    public int? EstimatedTravelMinutes { get; set; }
    public decimal? TurnoverCoefficientPerDay { get; set; }
    public bool IsActive { get; set; }
}

public sealed class TransportRouteWriteDto
{
    public int PlantId { get; set; }
    public int ConstructionObjectId { get; set; }
    public decimal? DistanceKm { get; set; }
    public int? EstimatedTravelMinutes { get; set; }
    public decimal? TurnoverCoefficientPerDay { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ConstructionTaktDto
{
    public long Id { get; set; }
    public int ConstructionObjectId { get; set; }
    public string? ConstructionObjectName { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Sequence { get; set; }
    public DateOnly PlannedProductionStartDate { get; set; }
    public DateOnly PlannedProductionEndDate { get; set; }
    public ConstructionTaktStatus Status { get; set; }
    public string? Comment { get; set; }
    public long Version { get; set; }
}

public sealed class ConstructionTaktWriteDto
{
    public int ConstructionObjectId { get; set; }
    [Required]
    public string Code { get; set; } = "";
    [Required]
    public string Name { get; set; } = "";
    public int Sequence { get; set; }
    public DateOnly PlannedProductionStartDate { get; set; }
    public DateOnly PlannedProductionEndDate { get; set; }
    public ConstructionTaktStatus Status { get; set; } = ConstructionTaktStatus.Draft;
    public string? Comment { get; set; }
}

public class ProductDto
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = "";
    public int ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }
    public string Mark { get; set; } = "";
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public decimal ThicknessMm { get; set; }
    public decimal CorniceWidthIncreaseMm { get; set; }
    public decimal TotalWidthWithCorniceMm { get; set; }
    public decimal ThicknessIncreaseMm { get; set; }
    public decimal RightBendMm { get; set; }
    public decimal LeftBendMm { get; set; }
    public decimal CladdingWidthWithBendsMm { get; set; }
    public decimal? WeightKg { get; set; }
    public ProductStatus Status { get; set; }
    public string? AdditionalInfo { get; set; }
    public long Version { get; set; }
}

public class ProductWriteDto
{
    [Required]
    public string ProductCode { get; set; } = "";
    public int ProductTypeId { get; set; }
    [Required]
    public string Mark { get; set; } = "";
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public decimal ThicknessMm { get; set; }
    public decimal CorniceWidthIncreaseMm { get; set; }
    public decimal TotalWidthWithCorniceMm { get; set; }
    public decimal ThicknessIncreaseMm { get; set; }
    public decimal RightBendMm { get; set; }
    public decimal LeftBendMm { get; set; }
    public decimal CladdingWidthWithBendsMm { get; set; }
    public decimal? WeightKg { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Created;
    public string? AdditionalInfo { get; set; }
}

public sealed class ConstructionObjectCreateDto : ConstructionObjectWriteDto
{
    [Range(0, 50)]
    public int SectionsCount { get; set; }
    [Range(0, 100)]
    public int FloorsCount { get; set; }
    [Range(0, 1000)]
    public int TaktsCount { get; set; }
    public bool AutoAddFloors { get; set; }
}
