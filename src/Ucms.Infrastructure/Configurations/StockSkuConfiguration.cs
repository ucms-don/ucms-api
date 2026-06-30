namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class StockSkuConfiguration : IEntityTypeConfiguration<StockSku>
{
    public void Configure(EntityTypeBuilder<StockSku> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("SkuId").IsRequired();
        builder.Property("StockId").IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
