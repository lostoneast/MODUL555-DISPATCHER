using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class ProductionAssignment
{
    public long Id { get; set; }


    // Потребность

    public long DemandRevisionId { get; set; }

    public DemandRevision DemandRevision { get; set; } = null!;


    // Выбранная линия

    public int ProductionLineId { get; set; }

    public ProductionLine ProductionLine { get; set; } = null!;


    // План

    public DateOnly PlannedProductionDate { get; set; }


    // Результат назначения

    public ProductionAssignmentMethod AssignmentMethod { get; set; }

    public ProductionAssignmentStatus Status { get; set; }


    /// <summary>
    /// Оценка варианта алгоритмом.
    /// Например 87.5.
    /// </summary>
    public decimal? Score { get; set; }


    /// <summary>
    /// Снимок причин решения алгоритма.
    /// PostgreSQL jsonb, но никакой логики внутри БД нет.
    /// </summary>
    public string? DecisionSnapshotJson { get; set; }


    /// <summary>
    /// Причина ручного изменения решения системы.
    /// </summary>
    public string? OverrideReason { get; set; }


    // История активности назначения

    public DateTimeOffset ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }


    // Системные поля

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByUserId { get; set; }
}