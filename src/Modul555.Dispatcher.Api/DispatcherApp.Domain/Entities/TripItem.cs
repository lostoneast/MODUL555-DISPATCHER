namespace DispatcherApp.Domain.Entities;

public class TripItem
{
    public long Id { get; set; }


    public long TripId { get; set; }

    public Trip Trip { get; set; } = null!;


    public long ProductId { get; set; }

    public Product Product { get; set; } = null!;


    /// <summary>
    /// Очередность размещения/погрузки.
    /// Используется для drag-and-drop во фронтенде.
    /// </summary>
    public int LoadingSequence { get; set; }


    public DateTimeOffset AddedAt { get; set; }

    public string? AddedByUserId { get; set; }
}