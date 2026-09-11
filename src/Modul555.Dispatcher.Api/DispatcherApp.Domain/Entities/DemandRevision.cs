using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class DemandRevision
{
    public long Id { get; set; }


    public long DemandId { get; set; }

    public Demand Demand { get; set; } = null!;


    public int RevisionNumber { get; set; }


    // Такт

    public long ConstructionTaktId { get; set; }

    public ConstructionTakt ConstructionTakt { get; set; } = null!;


    // Требования к производству

    /// <summary>
    /// Снимок начала производственного окна такта
    /// на момент создания данной версии потребности.
    /// </summary>
    public DateOnly RequiredProductionStartDate { get; set; }

    /// <summary>
    /// Снимок окончания производственного окна.
    /// </summary>
    public DateOnly RequiredProductionEndDate { get; set; }


    // Требования к доставке

    /// <summary>
    /// Раньше этой даты изделие желательно не доставлять.
    /// Например, из-за отсутствия места хранения.
    /// </summary>
    public DateOnly EarliestDeliveryDate { get; set; }

    /// <summary>
    /// Крайний срок доставки изделия.
    /// </summary>
    public DateOnly RequiredDeliveryDate { get; set; }


    // Приоритет

    public PriorityLevel PriorityLevel { get; set; }
        = PriorityLevel.Normal;

    /// <summary>
    /// Дополнительная сортировка внутри одного уровня приоритета.
    /// Чем меньше значение — тем раньше обрабатывать.
    /// </summary>
    public int PriorityOrder { get; set; }


    /// <summary>
    /// Причина создания новой версии.
    /// </summary>
    public string? ChangeReason { get; set; }

    public string? Comment { get; set; }


    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Стабильный идентификатор пользователя из Keycloak.
    /// </summary>
    public string? CreatedByUserId { get; set; }
}