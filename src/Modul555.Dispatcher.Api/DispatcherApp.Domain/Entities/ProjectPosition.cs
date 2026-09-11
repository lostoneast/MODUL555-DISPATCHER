namespace DispatcherApp.Domain.Entities;

public class ProjectPosition
{
    /// <summary>
    /// Одновременно PK и FK на Product.
    /// Обеспечивает связь Product 1:1 ProjectPosition.
    /// </summary>
    public long ProductId { get; set; }

    public Product Product { get; set; } = null!;


    public int ConstructionObjectId { get; set; }

    public ConstructionObject ConstructionObject { get; set; } = null!;


    public int? BuildingSectionId { get; set; }

    public BuildingSection? BuildingSection { get; set; }


    public int? FloorId { get; set; }

    public Floor? Floor { get; set; }


    public string? InstallationNumber { get; set; }
}