namespace Ucms.Domain.Common;

/// <summary>
/// Базовый класс для сущностей поддерживающих локализацию
/// </summary>
public abstract class LocalizableEntity : Entity, ILocalizableEntity
{
    /// <summary>
    /// Наименование на узбекском обязательно для заполнения
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Наименование на русском обязательно для заполнения
    /// </summary>
    public string NameRu { get; set; } = null!;

    /// <summary>
    /// Наименование на английском
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Наименование на каракалпакском
    /// </summary>
    public string? NameKa { get; set; }
}
