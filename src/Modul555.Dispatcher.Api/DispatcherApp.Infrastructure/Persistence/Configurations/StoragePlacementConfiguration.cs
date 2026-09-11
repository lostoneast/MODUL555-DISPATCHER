using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class StoragePlacementConfiguration : IEntityTypeConfiguration<StoragePlacement>
{
    public void Configure(EntityTypeBuilder<StoragePlacement> builder)
    {
        builder.ToTable("storage_placements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment).HasMaxLength(2000);

        builder
            .HasOne(x => x.Product)
            .WithMany(p => p.StoragePlacements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.StorageArea)
            .WithMany(a => a.Placements)
            .HasForeignKey(x => x.StorageAreaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
