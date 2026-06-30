namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class ClientActItemConfiguration : IEntityTypeConfiguration<ClientActItem>
{
    public void Configure(EntityTypeBuilder<ClientActItem> builder)
    {
        builder.HasIndex(e => e.ActId);
        builder.HasIndex(e => e.EstimateItemId);
        builder.Property(e => e.Volume).HasPrecision(28, 12);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
    }
}
