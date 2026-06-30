namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class IncomeItemConfiguration : IEntityTypeConfiguration<IncomeItem>
{
    public void Configure(EntityTypeBuilder<IncomeItem> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("IncomeId").IsRequired();
        builder.Property("SkuId").IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
