namespace Ucms.Domain.Common;

/// <summary>
/// Базовый класс для сущностей поддерживающих локализацию и аудит 
/// </summary>
public abstract class LocalizableAuditableEntity : Entity, ILocalizableEntity, IAuditableEntity
{
    /// <summary>
    /// Наименование на узбекском объязательно для заполнение
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Наименование на русском обязательно для заполненние
    /// </summary>
    public string NameRu { get; set; } = default!;

    /// <summary>
    /// Наименование на английском
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Наименование на каракалпакском
    /// </summary>
    public string? NameKa { get; set; }

    /// <summary>
    /// Запись создан в 
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Запись обновлен в
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Запис создан кем
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Запис обновлен кем
    /// </summary>
    public Guid UpdatedBy { get; set; }
}
