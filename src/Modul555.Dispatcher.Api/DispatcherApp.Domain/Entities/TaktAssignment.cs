namespace DispatcherApp.Domain.Entities;

public class TaktAssignment
{
    public long Id { get; set; }


    public long ConstructionTaktId { get; set; }

    public ConstructionTakt ConstructionTakt { get; set; } = null!;


    public long ProductId { get; set; }

    public Product Product { get; set; } = null!;


    public DateTimeOffset ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }


    public string? Comment { get; set; }
}