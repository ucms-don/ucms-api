namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Тип склада
/// </summary>
public enum StockType
{
    /// <summary>
    /// Не определен
    /// </summary>
    [Display(Name = "StockType_NotApplicable")]
    NotApplicable = 0,

    /// <summary>
    /// Здание
    /// </summary>
    [Display(Name = "StockType_Building")]
    Building = 10,

    /// <summary>
    /// Помещение
    /// </summary>
    [Display(Name = "StockType_Premises")]
    Premises = 20,

    /// <summary>
    /// Машина
    /// </summary>
    [Display(Name = "StockType_Car")]
    Car = 30,

    /// <summary>
    /// Чемодан
    /// </summary>
    [Display(Name = "StockType_Case")]
    Case = 40
}
