namespace DispatcherApp.Application.Common;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}

public sealed class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; } = true;
}

public sealed class LookupItem
{
    public required string Id { get; init; }
    public string? Code { get; init; }
    public required string Name { get; init; }
}

public sealed class ImportResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = [];
}
