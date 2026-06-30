namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Статус передачи расхода
/// </summary>
public enum OutcomeTransferStatus
{
    /// <summary>
    /// Отправлено
    /// </summary>
    [Display(Name = "OutcomeTransferStatus_Sent")]
    Sent = 0,

    /// <summary>
    /// Подтверждено
    /// </summary>
    [Display(Name = "OutcomeTransferStatus_Approved")]
    Approved = 20,

    /// <summary>
    /// Отклонено
    /// </summary>
    [Display(Name = "OutcomeTransferStatus_Cancelled")]
    Cancelled = 99,
}
