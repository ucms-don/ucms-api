namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class BrigadeConfiguration : IEntityTypeConfiguration<Brigade>
{
    public void Configure(EntityTypeBuilder<Brigade> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.OrganizationId);
        builder.Property(e => e.Name).HasMaxLength(512).IsRequired();
        builder.Property(e => e.ForemanName).HasMaxLength(512);
        builder.Property(e => e.Phone).HasMaxLength(64);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(e => e.WorkLogs)
            .WithOne(e => e.Brigade)
            .HasForeignKey(e => e.BrigadeId);

        builder.HasMany(e => e.Payments)
            .WithOne(e => e.Brigade)
            .HasForeignKey(e => e.BrigadeId);
    }
}
