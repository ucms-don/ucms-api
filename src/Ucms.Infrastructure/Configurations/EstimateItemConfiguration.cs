namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class EstimateItemConfiguration : IEntityTypeConfiguration<EstimateItem>
{
    public void Configure(EntityTypeBuilder<EstimateItem> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.SectionId);
        builder.Property(e => e.Description).HasMaxLength(2048);
        builder.Property(e => e.Volume).HasPrecision(28, 12);

        builder.HasOne(e => e.WorkType)
            .WithMany()
            .HasForeignKey(e => e.WorkTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MeasurementUnit)
            .WithMany()
            .HasForeignKey(e => e.MeasurementUnitId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.ClientUnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.BrigadeUnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.MaterialUnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.VatRate).HasPrecision(5, 2);

        builder.HasMany(e => e.WorkLogs)
            .WithOne(e => e.EstimateItem)
            .HasForeignKey(e => e.EstimateItemId);

        builder.HasMany(e => e.ClientActItems)
            .WithOne(e => e.EstimateItem)
            .HasForeignKey(e => e.EstimateItemId);
    }
}
