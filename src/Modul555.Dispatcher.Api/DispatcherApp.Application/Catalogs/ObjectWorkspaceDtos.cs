using System.ComponentModel.DataAnnotations;
using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Application.Catalogs;

public sealed class ObjectDeletionRequest
{
    [Required] public string ConfirmationName { get; set; } = "";
    [Required] public string PreviewToken { get; set; } = "";
}

public sealed record ObjectDeletionPreview(string Name, string PreviewToken,
    int ProductsToDelete, int ProductsToKeep, IReadOnlyDictionary<string, int> Records);

public sealed class ObjectProductsQuery
{
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 200)] public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? ProductTypeId { get; set; }
    public ProductStatus? Status { get; set; }
    public int? BuildingSectionId { get; set; }
    public int? FloorId { get; set; }
    public string SortBy { get; set; } = "productCode";
    public bool Descending { get; set; }
}

public sealed class ObjectProductWriteDto : ProductWriteDto
{
    [Range(1, long.MaxValue)] public long Version { get; set; } = 1;
    public int? BuildingSectionId { get; set; }
    public int? FloorId { get; set; }
    [MaxLength(64)] public string? InstallationNumber { get; set; }
}

public sealed class ObjectProductDto : ProductDto
{
    public int? BuildingSectionId { get; set; }
    public string? BuildingSectionName { get; set; }
    public int? FloorId { get; set; }
    public string? FloorName { get; set; }
    public string? InstallationNumber { get; set; }
    public string BindingSource { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed record ObjectStructureDto(IReadOnlyList<SectionOption> Sections, IReadOnlyList<FloorOption> Floors);
public sealed record SectionOption(int Id, string Name);
public sealed record FloorOption(int Id, int BuildingSectionId, string Name);
