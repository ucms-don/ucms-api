namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Тип прихода
/// </summary>
public enum IncomeType
{
    /// <summary>
    /// Внутренний
    /// </summary>
    [Display(Name = "IncomeType_Internal")]
    Internal = 10,

    /// <summary>
    /// Внешний
    /// </summary>
    [Display(Name = "IncomeType_External")]
    External = 20,

    /// <summary>
    /// Возврат
    /// </summary>
    [Display(Name = "IncomeType_Return")]
    Return = 30,
}
