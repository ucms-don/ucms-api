namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class AccountTransferConfiguration : IEntityTypeConfiguration<AccountTransfer>
{
    public void Configure(EntityTypeBuilder<AccountTransfer> builder)
    {
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.Date);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Commission).HasPrecision(18, 2);
        builder.Property(e => e.TransferredBy).HasMaxLength(256);
        builder.Property(e => e.Note).HasMaxLength(1024);

        builder.HasOne(e => e.FromAccount)
            .WithMany()
            .HasForeignKey(e => e.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToAccount)
            .WithMany()
            .HasForeignKey(e => e.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
