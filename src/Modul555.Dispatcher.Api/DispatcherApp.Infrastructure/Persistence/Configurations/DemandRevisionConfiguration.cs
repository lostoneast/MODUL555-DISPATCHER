using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class DemandRevisionConfiguration : IEntityTypeConfiguration<DemandRevision>
{
    public void Configure(EntityTypeBuilder<DemandRevision> builder)
    {
        builder.ToTable("demand_revisions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChangeReason).HasMaxLength(512);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.CreatedByUserId).HasMaxLength(128);

        builder.HasIndex(x => new { x.DemandId, x.RevisionNumber }).IsUnique();

        builder
            .HasOne(x => x.ConstructionTakt)
            .WithMany(t => t.DemandRevisions)
            .HasForeignKey(x => x.ConstructionTaktId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
