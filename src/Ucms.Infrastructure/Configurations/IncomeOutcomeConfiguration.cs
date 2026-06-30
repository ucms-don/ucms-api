namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class IncomeOutcomeConfiguration : IEntityTypeConfiguration<IncomeOutcome>
{
    public void Configure(EntityTypeBuilder<IncomeOutcome> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
