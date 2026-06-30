namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.ProjectId);
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.Property(e => e.Name).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1024);

        builder.HasMany(e => e.Sections)
            .WithOne(s => s.Estimate)
            .HasForeignKey(s => s.EstimateId);
    }
}
