using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Persistence;

public sealed class DispatcherDbContext : DbContext
{
    public DispatcherDbContext(DbContextOptions<DispatcherDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductIdentifier> ProductIdentifiers => Set<ProductIdentifier>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<ProjectPosition> ProjectPositions => Set<ProjectPosition>();
    public DbSet<ProductAssignment> ProductAssignments => Set<ProductAssignment>();
    public DbSet<ConstructionObject> ConstructionObjects => Set<ConstructionObject>();
    public DbSet<BuildingSection> BuildingSections => Set<BuildingSection>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<ConstructionTakt> ConstructionTakts => Set<ConstructionTakt>();
    public DbSet<TaktAssignment> TaktAssignments => Set<TaktAssignment>();
    public DbSet<Demand> Demands => Set<Demand>();
    public DbSet<DemandRevision> DemandRevisions => Set<DemandRevision>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<LineCapability> LineCapabilities => Set<LineCapability>();
    public DbSet<CapacityOverride> CapacityOverrides => Set<CapacityOverride>();
    public DbSet<ProductionAssignment> ProductionAssignments => Set<ProductionAssignment>();
    public DbSet<StorageArea> StorageAreas => Set<StorageArea>();
    public DbSet<StoragePlacement> StoragePlacements => Set<StoragePlacement>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripItem> TripItems => Set<TripItem>();
    public DbSet<UnloadingPoint> UnloadingPoints => Set<UnloadingPoint>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ApplyDecimalConventions(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DispatcherDbContext).Assembly);
    }

    private static void ApplyDecimalConventions(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model
                     .GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(3);
        }
    }
}
