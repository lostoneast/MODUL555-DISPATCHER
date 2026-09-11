using DispatcherApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherApp.Infrastructure.Persistence.Configurations;

internal sealed class TaktAssignmentConfiguration : IEntityTypeConfiguration<TaktAssignment>
{
    public void Configure(EntityTypeBuilder<TaktAssignment> builder)
    {
        builder.ToTable("takt_assignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment).HasMaxLength(2000);

        builder
            .HasOne(x => x.ConstructionTakt)
            .WithMany(t => t.TaktAssignments)
            .HasForeignKey(x => x.ConstructionTaktId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Product)
            .WithMany(p => p.TaktAssignments)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
