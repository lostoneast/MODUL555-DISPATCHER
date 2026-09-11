using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class UnloadingPointConfiguration : IEntityTypeConfiguration<UnloadingPoint>
{
    public void Configure(EntityTypeBuilder<UnloadingPoint> builder)
    {
        builder.ToTable("unloading_points");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder
            .HasOne(x => x.ConstructionObject)
            .WithMany()
            .HasForeignKey(x => x.ConstructionObjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
