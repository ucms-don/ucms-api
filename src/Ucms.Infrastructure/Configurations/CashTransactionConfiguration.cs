namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class CashTransactionConfiguration : IEntityTypeConfiguration<CashTransaction>
{
    public void Configure(EntityTypeBuilder<CashTransaction> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.CashAccountId);
        builder.HasIndex(e => new { e.PartnerType, e.PartnerId });
        builder.HasIndex(e => e.ProjectId);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Note).HasMaxLength(1024);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
