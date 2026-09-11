using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class Demand
{
    public long Id { get; set; }


    public long ProductId { get; set; }

    public Product Product { get; set; } = null!;


    public DemandStatus Status { get; set; }
        = DemandStatus.Draft;


    /// <summary>
    /// Ссылка на актуальную версию потребности.
    /// </summary>
    public long? CurrentRevisionId { get; set; }

    public DemandRevision? CurrentRevision { get; set; }


    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public long Version { get; set; }


    public ICollection<DemandRevision> Revisions { get; set; }
        = new List<DemandRevision>();
}