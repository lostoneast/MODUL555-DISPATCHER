namespace DispatcherApp.Domain.Enums;

public enum TripStatus
{
    Draft = 0,

    Planned = 10,
    Confirmed = 20,

    Loading = 30,
    Loaded = 40,

    InTransit = 50,

    Arrived = 60,
    Unloading = 70,

    Completed = 100,

    Cancelled = 900
}