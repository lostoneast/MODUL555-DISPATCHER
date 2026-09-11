using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class Trip
{
    public long Id { get; set; }


    public string TripNumber { get; set; } = null!;


    // Откуда

    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;


    // Куда

    public int ConstructionObjectId { get; set; }

    public ConstructionObject ConstructionObject { get; set; } = null!;


    public int? UnloadingPointId { get; set; }

    public UnloadingPoint? UnloadingPoint { get; set; }


    // Транспорт

    public long VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;


    // План

    public DateTimeOffset? PlannedLoadingAt { get; set; }

    public DateTimeOffset? PlannedDepartureAt { get; set; }

    public DateTimeOffset? PlannedArrivalAt { get; set; }


    // Факт

    public DateTimeOffset? ActualLoadingAt { get; set; }

    public DateTimeOffset? ActualDepartureAt { get; set; }

    public DateTimeOffset? ActualArrivalAt { get; set; }


    public TripStatus Status { get; set; }
        = TripStatus.Draft;


    public string? Comment { get; set; }


    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public long Version { get; set; }


    public ICollection<TripItem> Items { get; set; }
        = new List<TripItem>();
}