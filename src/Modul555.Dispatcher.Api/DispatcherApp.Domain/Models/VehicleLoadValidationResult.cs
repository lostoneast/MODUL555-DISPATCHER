public class VehicleLoadValidationResult
{
    public bool IsValid { get; set; }

    public decimal TotalWeightKg { get; set; }

    public decimal? MaxPayloadKg { get; set; }

    public List<string> Errors { get; set; } = new();

    public List<string> Warnings { get; set; } = new();
}