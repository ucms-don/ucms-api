namespace Ucms.Infrastructure.Configurations;

using Ucms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("Name").HasMaxLength(256).IsRequired();
        builder.Property("Note").HasMaxLength(1024).IsRequired(false);
        builder.Property("StockId").IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(e => e.IncomeOutcome)
            .WithOne(e => e.Income)
            .HasForeignKey<IncomeOutcome>(e => e.IncomeId)
            .IsRequired(false);
    }
}
