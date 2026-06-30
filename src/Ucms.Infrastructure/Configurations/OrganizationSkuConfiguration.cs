namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class OrganizationSkuConfiguration : IEntityTypeConfiguration<OrganizationSku>
{
    public void Configure(EntityTypeBuilder<OrganizationSku> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("SkuId").IsRequired();
        builder.Property("OrganizationId").IsRequired();
    }
}
