using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class DemandConfiguration : IEntityTypeConfiguration<Demand>
{
    public void Configure(EntityTypeBuilder<Demand> builder)
    {
        builder.ToTable("demands");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder
            .HasOne(x => x.Product)
            .WithMany(p => p.Demands)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Revisions)
            .WithOne(r => r.Demand)
            .HasForeignKey(r => r.DemandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CurrentRevision)
            .WithOne()
            .HasForeignKey<Demand>(x => x.CurrentRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
