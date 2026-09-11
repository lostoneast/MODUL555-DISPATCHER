namespace DispatcherApp.Domain.Entities;

public class StorageArea
{
    public int Id { get; set; }

    public int? PlantId { get; set; }

    public Plant? Plant { get; set; }

    public int? ConstructionObjectId { get; set; }

    public ConstructionObject? ConstructionObject { get; set; }


    public string Name { get; set; } = null!;

    /// <summary>
    /// Упрощенная емкость в количестве изделий.
    /// </summary>
    public int CapacityUnits { get; set; }

    public bool IsActive { get; set; } = true;


    public ICollection<StoragePlacement> Placements { get; set; }
        = new List<StoragePlacement>();
}