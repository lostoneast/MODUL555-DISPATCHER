namespace DispatcherApp.Domain.Entities;

public class ProductType
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; }
        = new List<Product>();

    public ICollection<LineCapability> LineCapabilities { get; set; }
        = new List<LineCapability>();
}