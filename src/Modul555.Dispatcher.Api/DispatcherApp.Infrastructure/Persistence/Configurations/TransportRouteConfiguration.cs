using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class TransportRouteConfiguration : IEntityTypeConfiguration<TransportRoute>
{
    public void Configure(EntityTypeBuilder<TransportRoute> builder)
    {
        builder.ToTable("transport_routes");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.PlantId, x.ConstructionObjectId }).IsUnique();

        builder
            .HasOne(x => x.Plant)
            .WithMany()
            .HasForeignKey(x => x.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ConstructionObject)
            .WithMany()
            .HasForeignKey(x => x.ConstructionObjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
