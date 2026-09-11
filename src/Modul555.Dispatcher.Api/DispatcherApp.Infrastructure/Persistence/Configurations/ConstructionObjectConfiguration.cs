using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class ConstructionObjectConfiguration : IEntityTypeConfiguration<ConstructionObject>
{
    public void Configure(EntityTypeBuilder<ConstructionObject> builder)
    {
        builder.ToTable("construction_objects");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
