namespace DispatcherApp.Domain.Entities;

public class ConstructionObject
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public bool IsActive { get; set; } = true;


    public ICollection<BuildingSection> Sections { get; set; }
        = new List<BuildingSection>();

    public ICollection<ProjectPosition> ProjectPositions { get; set; }
        = new List<ProjectPosition>();

    public ICollection<ProductAssignment> ProductAssignments { get; set; }
        = new List<ProductAssignment>();

    public ICollection<StorageArea> StorageAreas { get; set; }
        = new List<StorageArea>();
}