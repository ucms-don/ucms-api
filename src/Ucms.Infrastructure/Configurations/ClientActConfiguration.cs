namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class ClientActConfiguration : IEntityTypeConfiguration<ClientAct>
{
    public void Configure(EntityTypeBuilder<ClientAct> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.ProjectId);
        builder.Property(e => e.ActNumber).HasMaxLength(256).IsRequired();
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.Note).HasMaxLength(1024);

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Act)
            .HasForeignKey(e => e.ActId);

        builder.HasMany(e => e.Payments)
            .WithOne(e => e.Act)
            .HasForeignKey(e => e.ActId);
    }
}
