namespace DispatcherApp.Domain.Models;

public class ProductionCandidate
{
    public int ProductionLineId { get; set; }

    public int PlantId { get; set; }

    public DateOnly ProductionDate { get; set; }

    public int DailyCapacity { get; set; }

    public int AlreadyAssignedUnits { get; set; }

    public int RemainingCapacity { get; set; }

    public bool HasCapacity { get; set; }

    public bool StorageAvailable { get; set; }

    public decimal Score { get; set; }

    public List<string> Reasons { get; set; }
        = new();
}