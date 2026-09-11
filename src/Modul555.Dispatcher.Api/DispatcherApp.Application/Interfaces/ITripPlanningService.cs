namespace DispatcherApp.Application.Interfaces;

public interface ITripPlanningService
{
    Task<Trip> CreateTripAsync(
        int plantId,
        int constructionObjectId,
        long vehicleId,
        DateTimeOffset plannedLoadingAt,
        CancellationToken cancellationToken = default);

    Task AddProductAsync(
        long tripId,
        long productId,
        CancellationToken cancellationToken = default);

    Task RemoveProductAsync(
        long tripId,
        long productId,
        CancellationToken cancellationToken = default);

    Task ReorderProductsAsync(
        long tripId,
        IReadOnlyList<long> orderedProductIds,
        CancellationToken cancellationToken = default);
}