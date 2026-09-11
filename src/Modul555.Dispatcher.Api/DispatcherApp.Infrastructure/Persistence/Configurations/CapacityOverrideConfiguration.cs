using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class CapacityOverrideConfiguration : IEntityTypeConfiguration<CapacityOverride>
{
    public void Configure(EntityTypeBuilder<CapacityOverride> builder)
    {
        builder.ToTable("capacity_overrides");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(512);
        builder.HasIndex(x => new { x.LineCapabilityId, x.Date }).IsUnique();

        builder
            .HasOne(x => x.LineCapability)
            .WithMany(c => c.CapacityOverrides)
            .HasForeignKey(x => x.LineCapabilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
