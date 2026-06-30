namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class OutcomeItemConfiguration : IEntityTypeConfiguration<OutcomeItem>
{
    public void Configure(EntityTypeBuilder<OutcomeItem> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("OutcomeId").IsRequired();
        builder.Property("SkuId").IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
