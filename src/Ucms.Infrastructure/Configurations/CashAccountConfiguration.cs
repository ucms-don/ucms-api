namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class CashAccountConfiguration : IEntityTypeConfiguration<CashAccount>
{
    public void Configure(EntityTypeBuilder<CashAccount> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.OrganizationId);
        builder.Property(e => e.Name).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(1024);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(e => e.Transactions)
            .WithOne(e => e.CashAccount)
            .HasForeignKey(e => e.CashAccountId);
    }
}
