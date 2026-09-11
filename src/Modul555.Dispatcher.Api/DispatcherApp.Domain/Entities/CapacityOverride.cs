namespace DispatcherApp.Domain.Entities;

public class CapacityOverride
{
    public long Id { get; set; }

    public int LineCapabilityId { get; set; }

    public LineCapability LineCapability { get; set; } = null!;


    public DateOnly Date { get; set; }

    public int CapacityUnits { get; set; }

    public string? Reason { get; set; }
}