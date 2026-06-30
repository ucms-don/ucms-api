namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class WorkLogConfiguration : IEntityTypeConfiguration<WorkLog>
{
    public void Configure(EntityTypeBuilder<WorkLog> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.BrigadeId);
        builder.HasIndex(e => e.EstimateItemId);
        builder.HasIndex(e => e.BrigadePaymentId);

        builder.Property(e => e.Volume).HasPrecision(28, 12);
        builder.Property(e => e.BrigadeUnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.Note).HasMaxLength(1024);
    }
}
