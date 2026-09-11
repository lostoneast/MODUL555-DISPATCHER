namespace DispatcherApp.Domain.Entities;

public class UnloadingPoint
{
    public int Id { get; set; }

    public int ConstructionObjectId { get; set; }

    public ConstructionObject ConstructionObject { get; set; } = null!;


    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;


    public ICollection<Trip> Trips { get; set; }
        = new List<Trip>();
}