namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Статус расхода
/// </summary>
public enum OutcomeStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    [Display(Name = "OutcomeStatus_Draft")]
    Draft = 0,

    /// <summary>
    /// Подтвержден
    /// </summary>
    [Display(Name = "OutcomeStatus_Approved")]
    Approved = 20,

    /// <summary>
    /// Отменен
    /// </summary>
    [Display(Name = "OutcomeStatus_Cancelled")]
    Cancelled = 99,
}
