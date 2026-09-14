namespace DispatcherApp.Application.Catalogs;

public sealed class ProductDetailWriteDto : ObjectProductWriteDto
{
    public int? ConstructionObjectId { get; set; }
    public long? TripId { get; set; }
}

public sealed record ProductTripOption(long Id, string TripNumber, string Status, string PlantName,
    string Vehicle, DateTimeOffset? PlannedArrivalAt);

public sealed record ProductHistoryEntry(string Kind, long Id, DateTimeOffset Date,
    DateTimeOffset? EndDate, string Description, string? Comment);

public sealed record ProductDetailDto(ProductListDto Product, IReadOnlyList<ProductHistoryEntry> History);
