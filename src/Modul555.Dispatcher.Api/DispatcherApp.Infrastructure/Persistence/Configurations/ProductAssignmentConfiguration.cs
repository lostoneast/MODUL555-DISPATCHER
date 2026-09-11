using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class ProductAssignmentConfiguration : IEntityTypeConfiguration<ProductAssignment>
{
    public void Configure(EntityTypeBuilder<ProductAssignment> builder)
    {
        builder.ToTable("product_assignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InstallationNumber).HasMaxLength(64);
        builder.Property(x => x.Comment).HasMaxLength(2000);

        builder
            .HasOne(x => x.Product)
            .WithMany(p => p.Assignments)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ConstructionObject)
            .WithMany(o => o.ProductAssignments)
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
