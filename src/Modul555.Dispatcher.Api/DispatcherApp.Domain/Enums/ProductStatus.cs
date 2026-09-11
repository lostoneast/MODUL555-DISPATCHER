namespace DispatcherApp.Domain.Enums;

public enum ProductStatus
{
    Created = 0,
    InProduction = 10,
    InStorage = 20,
    AssignedToTrip = 30,
    InTransit = 40,
    Delivered = 100,
    Cancelled = 900,
}
