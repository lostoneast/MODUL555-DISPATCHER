namespace DispatcherApp.Domain.Entities;

public class LineCapability
{
    public int Id { get; set; }

    public int ProductionLineId { get; set; }

    public ProductionLine ProductionLine { get; set; } = null!;


    public int ProductTypeId { get; set; }

    public ProductType ProductType { get; set; } = null!;


    /// <summary>
    /// Номинальная производительность в изделиях в сутки.
    /// </summary>
    public int DefaultDailyCapacityUnits { get; set; }


    public bool IsActive { get; set; } = true;


    public ICollection<CapacityOverride> CapacityOverrides { get; set; }
        = new List<CapacityOverride>();
}