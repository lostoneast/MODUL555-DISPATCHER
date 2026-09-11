using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class ConstructionTaktConfiguration : IEntityTypeConfiguration<ConstructionTakt>
{
    public void Configure(EntityTypeBuilder<ConstructionTakt> builder)
    {
        builder.ToTable("construction_takts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder
            .HasOne(x => x.ConstructionObject)
            .WithMany()
            .HasForeignKey(x => x.ConstructionObjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
