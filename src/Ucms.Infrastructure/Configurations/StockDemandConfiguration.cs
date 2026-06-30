namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class StockDemandConfiguration : IEntityTypeConfiguration<StockDemand>
{
    public void Configure(EntityTypeBuilder<StockDemand> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("Name").HasMaxLength(256).IsRequired();
        builder.Property("Note").HasMaxLength(1024).IsRequired(false);
        builder.Property("SenderId").IsRequired();
        builder.Property("RecipientId").IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
