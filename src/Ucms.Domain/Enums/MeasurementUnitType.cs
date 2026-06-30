namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Тип единицы измерении
/// </summary>
public enum MeasurementUnitType
{
    /// <summary>
    /// Не определен
    /// </summary>
    [Display(Name = "MeasurementUnitType_Undefined")]
    Undefined = 0,

    /// <summary>
    /// Длина
    /// </summary>
    [Display(Name = "MeasurementUnitType_Length")]
    Length = 10,

    /// <summary>
    /// Вес
    /// </summary>
    [Display(Name = "MeasurementUnitType_Weight")]
    Weight = 20,

    /// <summary>
    /// Количество
    /// </summary>
    [Display(Name = "MeasurementUnitType_Quantity")]
    Quantity = 30,

    /// <summary>
    /// Количество Ампула
    /// </summary>
    [Display(Name = "MeasurementUnitType_Quantity_Ampoule")]
    QuantityAmpoule = 31,

    /// <summary>
    /// Количество Таблетка
    /// </summary>
    [Display(Name = "MeasurementUnitType_Quantity_Pill")]
    QuantityPill = 32,

    /// <summary>
    /// Объем
    /// </summary>
    [Display(Name = "MeasurementUnitType_Volume")]
    Volume = 40,

    /// <summary>
    /// Капли
    /// </summary>
    [Display(Name = "MeasurementUnitType_Drops")]
    Drops = 41
}
