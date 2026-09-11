namespace DispatcherApp.Domain.Entities;

public class BuildingSection
{
    public int Id { get; set; }

    public int ConstructionObjectId { get; set; }

    public ConstructionObject ConstructionObject { get; set; } = null!;


    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }


    public ICollection<Floor> Floors { get; set; }
        = new List<Floor>();
}