namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Категория склада
/// </summary>
public enum StockCategory
{
    /// <summary>
    /// По Умолчанию
    /// </summary>
    [Display(Name = "StockCategory_Default")]
    Default = 0,

    /// <summary>
    /// Центральный
    /// </summary>
    [Display(Name = "StockCategory_Central")]
    Central = 10
}
