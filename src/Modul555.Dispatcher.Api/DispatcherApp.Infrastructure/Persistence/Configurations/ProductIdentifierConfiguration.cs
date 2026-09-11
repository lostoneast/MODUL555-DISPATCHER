using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class ProductIdentifierConfiguration : IEntityTypeConfiguration<ProductIdentifier>
{
    public void Configure(EntityTypeBuilder<ProductIdentifier> builder)
    {
        builder.ToTable("product_identifiers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => new { x.Type, x.Value }).IsUnique();

        builder
            .HasOne(x => x.Product)
            .WithMany(p => p.Identifiers)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
