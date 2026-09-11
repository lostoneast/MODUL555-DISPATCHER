namespace DispatcherApp.Domain.Entities;

public class Plant
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<ProductionLine> ProductionLines { get; set; }
        = new List<ProductionLine>();
    
    public ICollection<StorageArea> StorageAreas { get; set; }
    = new List<StorageArea>();
}