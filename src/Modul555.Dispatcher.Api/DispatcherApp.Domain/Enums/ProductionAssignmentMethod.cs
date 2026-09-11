namespace DispatcherApp.Domain.Enums;

public enum ProductionAssignmentMethod
{
    Automatic = 10,

    /// <summary>
    /// Диспетчер назначил производство самостоятельно.
    /// </summary>
    Manual = 20,

    /// <summary>
    /// Система предложила вариант,
    /// диспетчер его переопределил.
    /// </summary>
    ManualOverride = 30
}