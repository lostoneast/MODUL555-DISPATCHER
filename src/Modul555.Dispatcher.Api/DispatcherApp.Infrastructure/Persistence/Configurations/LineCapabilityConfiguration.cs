using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class LineCapabilityConfiguration : IEntityTypeConfiguration<LineCapability>
{
    public void Configure(EntityTypeBuilder<LineCapability> builder)
    {
        builder.ToTable("line_capabilities");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ProductionLineId, x.ProductTypeId }).IsUnique();

        builder
            .HasOne(x => x.ProductionLine)
            .WithMany(l => l.Capabilities)
            .HasForeignKey(x => x.ProductionLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ProductType)
            .WithMany(t => t.LineCapabilities)
            .HasForeignKey(x => x.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
