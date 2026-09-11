using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class ProjectPositionConfiguration : IEntityTypeConfiguration<ProjectPosition>
{
    public void Configure(EntityTypeBuilder<ProjectPosition> builder)
    {
        builder.ToTable("project_positions");
        builder.HasKey(x => x.ProductId);

        builder.Property(x => x.InstallationNumber).HasMaxLength(64);

        builder
            .HasOne(x => x.Product)
            .WithOne(p => p.ProjectPosition)
            .HasForeignKey<ProjectPosition>(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ConstructionObject)
            .WithMany(o => o.ProjectPositions)
            .HasForeignKey(x => x.ConstructionObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.BuildingSection)
            .WithMany()
            .HasForeignKey(x => x.BuildingSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Floor)
            .WithMany()
            .HasForeignKey(x => x.FloorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
