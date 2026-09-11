public interface IVehicleLoadValidationService
{
    Task<VehicleLoadValidationResult> ValidateAsync(
        long vehicleId,
        IEnumerable<long> productIds,
        CancellationToken cancellationToken = default);
}