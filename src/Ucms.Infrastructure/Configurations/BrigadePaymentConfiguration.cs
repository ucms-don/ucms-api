namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class BrigadePaymentConfiguration : IEntityTypeConfiguration<BrigadePayment>
{
    public void Configure(EntityTypeBuilder<BrigadePayment> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.BrigadeId);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Note).HasMaxLength(1024);

        builder.HasMany(e => e.WorkLogs)
            .WithOne(e => e.BrigadePayment)
            .HasForeignKey(e => e.BrigadePaymentId)
            .IsRequired(false);
    }
}
