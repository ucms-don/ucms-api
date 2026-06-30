namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Infrastructure.Configurations.Base;
using Ucms.Domain.Entities;

public class SupplierConfiguration : LocalizableConfiguration<Supplier>
{
    public override void Configure(EntityTypeBuilder<Supplier> builder)
    {
        base.Configure(builder);

        builder.Property("Code").HasMaxLength(32).IsRequired(true);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

