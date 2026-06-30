namespace Ucms.Infrastructure.Configurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Common;

/// <summary>
/// Базовый класс конфигурации для сущностей поддерживающих локализации
/// </summary>
/// <typeparam name="TEntity">Класс производный от LocalizableEntity</typeparam>
public abstract class LocalizableConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : LocalizableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property("Name").HasMaxLength(512).IsRequired();
        builder.Property("NameRu").HasMaxLength(512).IsRequired();
        builder.Property("NameEn").HasMaxLength(512).IsRequired(false);
        builder.Property("NameKa").HasMaxLength(512).IsRequired(false);
    }
}
