namespace DispatcherApp.Domain.Entities;

public class StoragePlacement
{
    public long Id { get; set; }


    public long ProductId { get; set; }

    public Product Product { get; set; } = null!;


    public int StorageAreaId { get; set; }

    public StorageArea StorageArea { get; set; } = null!;


    public DateTimeOffset ArrivedAt { get; set; }

    public DateTimeOffset? DepartedAt { get; set; }


    public string? Comment { get; set; }
}