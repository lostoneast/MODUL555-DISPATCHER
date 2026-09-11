using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class ConstructionTakt
{
    public long Id { get; set; }

    public int ConstructionObjectId { get; set; }

    public ConstructionObject ConstructionObject { get; set; } = null!;


    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>
    /// Порядок такта внутри объекта.
    /// </summary>
    public int Sequence { get; set; }


    /// <summary>
    /// Начало требуемого окна изготовления изделий такта.
    /// </summary>
    public DateOnly PlannedProductionStartDate { get; set; }

    /// <summary>
    /// Конец требуемого окна изготовления изделий такта.
    /// </summary>
    public DateOnly PlannedProductionEndDate { get; set; }


    public ConstructionTaktStatus Status { get; set; }
        = ConstructionTaktStatus.Draft;

    public string? Comment { get; set; }


    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public long Version { get; set; }


    public ICollection<TaktAssignment> TaktAssignments { get; set; }
        = new List<TaktAssignment>();

    public ICollection<DemandRevision> DemandRevisions { get; set; }
        = new List<DemandRevision>();
}