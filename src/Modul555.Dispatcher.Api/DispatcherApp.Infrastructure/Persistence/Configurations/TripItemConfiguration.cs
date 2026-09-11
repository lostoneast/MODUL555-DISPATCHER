using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class TripItemConfiguration : IEntityTypeConfiguration<TripItem>
{
    public void Configure(EntityTypeBuilder<TripItem> builder)
    {
        builder.ToTable("trip_items");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AddedByUserId).HasMaxLength(128);
        builder.HasIndex(x => new { x.TripId, x.ProductId }).IsUnique();

        builder
            .HasOne(x => x.Trip)
            .WithMany(t => t.Items)
            .HasForeignKey(x => x.TripId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Product)
            .WithMany(p => p.TripItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
