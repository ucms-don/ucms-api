namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(512).IsRequired();
        builder.Property(e => e.TaxId).HasMaxLength(64);
        builder.Property(e => e.Address).HasMaxLength(1024);
        builder.Property(e => e.Phone).HasMaxLength(64);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Type).HasDefaultValue(Ucms.Domain.Enums.OrganizationType.Tenant);
        builder.Property(e => e.IsTest).HasDefaultValue(false);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(e => e.Projects)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId);

        builder.HasMany(e => e.Brigades)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId);
    }
}
