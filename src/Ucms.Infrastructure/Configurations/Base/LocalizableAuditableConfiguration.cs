namespace Ucms.Infrastructure.Configurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Common;

/// <summary>
/// Базовый класс конфигурации для сущностей поддерживающих локализации и аудит
/// </summary>
/// <typeparam name="TEntity">Класс производный от LocalizableAuditableEntity</typeparam>
public abstract class LocalizableAuditableConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : LocalizableAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("Name").HasMaxLength(256).IsRequired();
        builder.Property("NameRu").HasMaxLength(256).IsRequired();
        builder.Property("NameEn").HasMaxLength(256).IsRequired(false);
        builder.Property("NameKa").HasMaxLength(256).IsRequired(false);

        builder.Property("CreatedBy").IsRequired(false);
        builder.Property("UpdatedBy").IsRequired(false);
        builder.Property("CreatedAt").IsRequired();
        builder.Property("UpdatedAt").IsRequired();
    }
}
