namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class EstimateSectionConfiguration : IEntityTypeConfiguration<EstimateSection>
{
    public void Configure(EntityTypeBuilder<EstimateSection> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.EstimateId);
        builder.Property(e => e.Name).HasMaxLength(512).IsRequired();

        builder.HasMany(e => e.EstimateItems)
            .WithOne(e => e.Section)
            .HasForeignKey(e => e.SectionId);
    }
}
