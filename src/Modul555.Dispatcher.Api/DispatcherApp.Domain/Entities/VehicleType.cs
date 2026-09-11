namespace DispatcherApp.Domain.Entities;

public class VehicleType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? RollingStockType { get; set; }


    // Грузоподъемность

    public decimal? MinPayloadKg { get; set; }

    public decimal? MaxPayloadKg { get; set; }


    // Погрузочные площадки

    public int LoadingPlatformCount { get; set; }

    public decimal? LoadingPlatformLengthMm { get; set; }

    public decimal? LoadingPlatformWidthMm { get; set; }


    // Допустимый перевес

    public decimal? AllowedRightLeftImbalanceKg { get; set; }


    // Габариты груза

    public decimal? MaxCargoHeightMm { get; set; }

    public decimal? CargoVolumeM3 { get; set; }


    // Габариты автопоезда

    public decimal? TotalTrainLengthMm { get; set; }

    public decimal? TurningRadiusMm { get; set; }


    public string? Notes { get; set; }


    public ICollection<Vehicle> Vehicles { get; set; }
        = new List<Vehicle>();
}