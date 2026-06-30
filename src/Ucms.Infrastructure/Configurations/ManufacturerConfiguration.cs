namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Infrastructure.Configurations.Base;
using Ucms.Domain.Entities;

public class ManufacturerConfiguration : LocalizableConfiguration<Manufacturer>
{
    public override void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        base.Configure(builder);

        builder.Property("Code").HasMaxLength(32).IsRequired(false);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
