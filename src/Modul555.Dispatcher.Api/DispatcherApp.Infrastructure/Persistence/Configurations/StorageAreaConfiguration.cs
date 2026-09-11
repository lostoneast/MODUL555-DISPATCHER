using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class StorageAreaConfiguration : IEntityTypeConfiguration<StorageArea>
{
    public void Configure(EntityTypeBuilder<StorageArea> builder)
    {
        builder.ToTable("storage_areas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();

        builder
            .HasOne(x => x.Plant)
            .WithMany(p => p.StorageAreas)
            .HasForeignKey(x => x.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ConstructionObject)
            .WithMany(o => o.StorageAreas)
            .HasForeignKey(x => x.ConstructionObjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
