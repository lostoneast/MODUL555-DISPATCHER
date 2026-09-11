namespace DispatcherApp.Application.Interfaces;

public interface IStorageCapacityService
{
    Task<int> GetCapacityAsync(
        int storageAreaId,
        CancellationToken cancellationToken = default);

    Task<int> GetOccupiedUnitsAsync(
        int storageAreaId,
        CancellationToken cancellationToken = default);

    Task<int> GetAvailableUnitsAsync(
        int storageAreaId,
        CancellationToken cancellationToken = default);

    Task<bool> CanAcceptAsync(
        int storageAreaId,
        int units,
        CancellationToken cancellationToken = default);
}