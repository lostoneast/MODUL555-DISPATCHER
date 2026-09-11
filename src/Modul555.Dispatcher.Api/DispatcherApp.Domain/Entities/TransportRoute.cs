namespace DispatcherApp.Domain.Entities;

public class TransportRoute
{
    public int Id { get; set; }


    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;


    public int ConstructionObjectId { get; set; }

    public ConstructionObject ConstructionObject { get; set; } = null!;


    public decimal? DistanceKm { get; set; }

    public int? EstimatedTravelMinutes { get; set; }


    /// <summary>
    /// Среднее количество оборотов транспортного средства в сутки.
    /// Например 1.5 или 2.
    /// </summary>
    public decimal? TurnoverCoefficientPerDay { get; set; }


    public bool IsActive { get; set; } = true;
}