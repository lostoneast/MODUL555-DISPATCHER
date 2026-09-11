public interface IProductionCapacityService
{
    Task<int> GetCapacityAsync(
        int productionLineId,
        int productTypeId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<int> GetAssignedUnitsAsync(
        int productionLineId,
        int productTypeId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<int> GetAvailableCapacityAsync(
        int productionLineId,
        int productTypeId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}