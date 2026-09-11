using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class AuditEvent
{
    public long Id { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Стабильный идентификатор пользователя из Keycloak.</summary>
    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string EntityType { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string Action { get; set; } = null!;

    /// <summary>PostgreSQL jsonb.</summary>
    public string? OldValuesJson { get; set; }

    /// <summary>PostgreSQL jsonb.</summary>
    public string? NewValuesJson { get; set; }

    public string? Comment { get; set; }

    public AuditSource Source { get; set; }

    public string? CorrelationId { get; set; }
}
