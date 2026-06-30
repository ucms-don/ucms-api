namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class ClientPaymentConfiguration : IEntityTypeConfiguration<ClientPayment>
{
    public void Configure(EntityTypeBuilder<ClientPayment> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.ActId);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Note).HasMaxLength(1024);
    }
}
