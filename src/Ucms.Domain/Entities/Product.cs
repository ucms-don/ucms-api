namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Справочник товаров
/// </summary>
public class Product : LocalizableEntity, IDeletable
{
    /// <summary>
    /// Код товара
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Международный код
    /// </summary>
    public string? InternationalCode { get; set; }

    /// <summary>
    /// Международный наименование
    /// </summary>
    public string? InternationalName { get; set; }

    /// <summary>
    /// Альтернативное наименование
    /// </summary>
    public string? AlternativeName { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Тип продукта
    /// </summary>
    public ProductType Type { get; set; }

    public virtual ICollection<StockBalanceRegister> StockBalanceRegistery { get; set; } = [];
}
