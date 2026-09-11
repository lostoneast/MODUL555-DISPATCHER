namespace DispatcherApp.Domain.Entities;

public class ProductionLine
{
    public int Id { get; set; }

    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;


    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;


    public ICollection<LineCapability> Capabilities { get; set; }
        = new List<LineCapability>();

    public ICollection<ProductionAssignment> ProductionAssignments { get; set; }
        = new List<ProductionAssignment>();
}