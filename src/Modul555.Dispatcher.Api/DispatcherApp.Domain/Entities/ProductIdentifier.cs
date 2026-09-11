using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class ProductIdentifier
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public Product Product { get; set; } = null!;


    public ProductIdentifierType Type { get; set; }

    public string Value { get; set; } = null!;


    public bool IsActive { get; set; } = true;


    public DateTimeOffset AssignedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}