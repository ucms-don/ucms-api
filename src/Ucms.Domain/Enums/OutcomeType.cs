namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Тип расхода
/// </summary>
public enum OutcomeType
{
    /// <summary>
    /// Передача
    /// </summary>
    [Display(Name = "OutcomeType_Broadcast")]
    Broadcast = 10,

    /// <summary>
    /// Использование
    /// </summary>
    [Display(Name = "OutcomeType_Usage")]
    Usage = 20,

    /// <summary>
    /// Списание
    /// </summary>
    [Display(Name = "OutcomeType_WriteOff")]
    WriteOff = 30,

    /// <summary>
    /// Возврат
    /// </summary>
    [Display(Name = "OutcomeType_Return")]
    Return = 50,
}
