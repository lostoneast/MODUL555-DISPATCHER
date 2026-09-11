namespace DispatcherApp.Domain.Enums;

public enum DemandStatus
{
    Draft = 0,

    Active = 10,

    /// <summary>
    /// Потребность уже включена в производственный/логистический план.
    /// </summary>
    Scheduled = 20,

    Fulfilled = 100,

    Cancelled = 900
}