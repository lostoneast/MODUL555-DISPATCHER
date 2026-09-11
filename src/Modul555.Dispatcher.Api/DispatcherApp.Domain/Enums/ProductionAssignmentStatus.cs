namespace DispatcherApp.Domain.Enums;

public enum ProductionAssignmentStatus
{
    Proposed = 10,

    Assigned = 20,

    /// <summary>
    /// Назначение существует, но доступная
    /// мощность линии превышена.
    /// </summary>
    CapacityConflict = 30,

    Confirmed = 40,

    InProduction = 50,

    Completed = 100,

    Cancelled = 900
}