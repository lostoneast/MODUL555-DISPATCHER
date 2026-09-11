using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.ProductCode).IsUnique();

        builder.Property(x => x.Mark).HasMaxLength(128).IsRequired();
        builder.Property(x => x.AdditionalInfo).HasMaxLength(2000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder
            .HasOne(x => x.ProductType)
            .WithMany(t => t.Products)
            .HasForeignKey(x => x.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
