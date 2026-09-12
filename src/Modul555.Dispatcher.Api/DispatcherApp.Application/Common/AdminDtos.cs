using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Application.Common;

public sealed class AdminOverviewDto
{
    public int Products { get; set; }
    public int ProductTypes { get; set; }
    public int Plants { get; set; }
    public int ProductionLines { get; set; }
    public int ConstructionObjects { get; set; }
    public int ConstructionTakts { get; set; }
    public int Vehicles { get; set; }
    public int StorageAreas { get; set; }
    public int Trips { get; set; }
    public int AuditEvents { get; set; }
}

public sealed class RoleOptionDto
{
    public required string Value { get; init; }
    public required string Label { get; init; }
}

public sealed class AuditEventDto
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Comment { get; set; }
    public AuditSource Source { get; set; }
    public string? CorrelationId { get; set; }
}
