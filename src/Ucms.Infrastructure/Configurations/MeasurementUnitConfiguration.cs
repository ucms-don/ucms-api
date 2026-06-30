namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Infrastructure.Configurations.Base;
using Ucms.Domain.Entities;

public class MeasurementUnitConfiguration : LocalizableConfiguration<MeasurementUnit>
{
    public override void Configure(EntityTypeBuilder<MeasurementUnit> builder)
    {
        base.Configure(builder);
        builder.Property("Code").HasMaxLength(32).IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
