namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Infrastructure.Configurations.Base;
using Ucms.Domain.Entities;

public class WorkTypeConfiguration : LocalizableConfiguration<WorkType>
{
    public override void Configure(EntityTypeBuilder<WorkType> builder)
    {
        base.Configure(builder);
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(e => e.Code).HasMaxLength(32);
        builder.HasIndex(e => e.Code).IsUnique();
    }
}
