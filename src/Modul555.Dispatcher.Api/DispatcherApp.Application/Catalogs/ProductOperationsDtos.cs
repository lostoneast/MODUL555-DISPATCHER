using System.ComponentModel.DataAnnotations;
using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Application.Catalogs;

public class ProductOperationDto
{
    [Range(1, long.MaxValue)] public long ProductVersion { get; set; }
    [Required, MaxLength(512)] public string Reason { get; set; } = "";
}

public sealed class ProductIdentifierWriteDto : ProductOperationDto
{
    [EnumDataType(typeof(ProductIdentifierType))] public ProductIdentifierType Type { get; set; }
    [Required, MaxLength(256)] public string Value { get; set; } = "";
}

public sealed class ProductDemandWriteDto : ProductOperationDto
{
    public long? DemandVersion { get; set; }
    [Range(1, long.MaxValue)] public long ConstructionTaktId { get; set; }
    public DateOnly EarliestDeliveryDate { get; set; }
    public DateOnly RequiredDeliveryDate { get; set; }
    [EnumDataType(typeof(PriorityLevel))] public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.Normal;
    [Range(0, int.MaxValue)] public int PriorityOrder { get; set; }
    public DemandStatus Status { get; set; } = DemandStatus.Active;
    [MaxLength(2000)] public string? Comment { get; set; }
}

public sealed class ProductDemandCancelDto : ProductOperationDto
{
    [Range(1, long.MaxValue)] public long DemandVersion { get; set; }
}

public sealed record ProductDemandRevisionDto(long Id, int RevisionNumber, long ConstructionTaktId, string TaktName,
    DateOnly RequiredProductionStartDate, DateOnly RequiredProductionEndDate, DateOnly EarliestDeliveryDate,
    DateOnly RequiredDeliveryDate, PriorityLevel PriorityLevel, int PriorityOrder, string? ChangeReason,
    string? Comment, DateTimeOffset CreatedAt, string? CreatedByUserId);

public sealed record ProductDemandDto(long Id, long Version, DemandStatus Status, long? CurrentRevisionId,
    bool HasProductionAssignments, IReadOnlyList<ProductDemandRevisionDto> Revisions);

public sealed record ProductDemandTaktOption(long Id, string Name, DateOnly ProductionStartDate, DateOnly ProductionEndDate);
