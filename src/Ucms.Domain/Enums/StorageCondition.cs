namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Условие хранение
/// </summary>
public enum StorageCondition
{
    /// <summary>
    /// Сухой склад
    /// </summary>
    [Display(Name = "StorageCondition_Dry")]
    Dry = 10,

    /// <summary>
    /// Холодильный склад (+4)
    /// </summary>
    [Display(Name = "StorageCondition_Refrigerated")]
    Refrigerated = 20,

    /// <summary>
    /// Морозильный склад (-20)
    /// </summary>
    [Display(Name = "StorageCondition_Freezer")]
    Freezer = 30,

    /// <summary>
    /// Склад глубокой заморозки (-70)
    /// </summary>
    [Display(Name = "StorageCondition_DeepFreeze")]
    DeepFreeze = 40
}
