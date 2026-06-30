namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.OrganizationId);
        builder.Property(e => e.Name).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(64);
        builder.Property(e => e.TaxId).HasMaxLength(64);
        builder.Property(e => e.Address).HasMaxLength(1024);
        builder.Property(e => e.Notes).HasMaxLength(1024);
        builder.Property(e => e.DirectorName).HasMaxLength(256);
        builder.Property(e => e.DirectorPosition).HasMaxLength(128);
        builder.Property(e => e.DirectorPhone).HasMaxLength(64);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(e => e.Projects)
            .WithOne(e => e.Customer)
            .HasForeignKey(e => e.CustomerId)
            .IsRequired(false);
    }
}
