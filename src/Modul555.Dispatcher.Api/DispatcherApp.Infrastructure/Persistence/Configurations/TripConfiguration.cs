using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TripNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.TripNumber).IsUnique();

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

        builder
            .HasOne(x => x.UnloadingPoint)
            .WithMany(u => u.Trips)
            .HasForeignKey(x => x.UnloadingPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Vehicle)
            .WithMany(v => v.Trips)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
