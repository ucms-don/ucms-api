namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class OutcomeConfiguration : IEntityTypeConfiguration<Outcome>
{
    public void Configure(EntityTypeBuilder<Outcome> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("Name").HasMaxLength(256).IsRequired();
        builder.Property("Note").HasMaxLength(1024).IsRequired(false);
        builder.Property("StockId").IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(e => e.IncomeOutcome)
            .WithOne(e => e.Outcome)
            .HasForeignKey<IncomeOutcome>(e => e.OutcomeId)
            .IsRequired(false);
    }
}
