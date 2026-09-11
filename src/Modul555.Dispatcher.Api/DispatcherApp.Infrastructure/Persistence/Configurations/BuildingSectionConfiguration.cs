using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class BuildingSectionConfiguration : IEntityTypeConfiguration<BuildingSection>
{
    public void Configure(EntityTypeBuilder<BuildingSection> builder)
    {
        builder.ToTable("building_sections");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();

        builder
            .HasOne(x => x.ConstructionObject)
            .WithMany(o => o.Sections)
            .HasForeignKey(x => x.ConstructionObjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
