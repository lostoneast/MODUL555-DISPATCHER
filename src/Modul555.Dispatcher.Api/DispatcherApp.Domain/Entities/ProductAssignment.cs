using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class ProductAssignment
{
    public long Id { get; set; }


    public long ProductId { get; set; }

    public Product Product { get; set; } = null!;


    public int ConstructionObjectId { get; set; }

    public ConstructionObject ConstructionObject { get; set; } = null!;


    public int? BuildingSectionId { get; set; }

    public BuildingSection? BuildingSection { get; set; }


    public int? FloorId { get; set; }

    public Floor? Floor { get; set; }


    public string? InstallationNumber { get; set; }


    public AssignmentReason Reason { get; set; }

    public string? Comment { get; set; }


    public DateTimeOffset ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }
}