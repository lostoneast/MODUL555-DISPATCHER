using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class ProductionAssignmentConfiguration
    : IEntityTypeConfiguration<ProductionAssignment>
{
    public void Configure(EntityTypeBuilder<ProductionAssignment> builder)
    {
        builder.ToTable("production_assignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score).HasPrecision(10, 3);
        builder.Property(x => x.DecisionSnapshotJson).HasColumnType("jsonb");
        builder.Property(x => x.OverrideReason).HasMaxLength(2000);
        builder.Property(x => x.CreatedByUserId).HasMaxLength(128);

        builder
            .HasOne(x => x.DemandRevision)
            .WithMany(r => r.ProductionAssignments)
            .HasForeignKey(x => x.DemandRevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ProductionLine)
            .WithMany(l => l.ProductionAssignments)
            .HasForeignKey(x => x.ProductionLineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
