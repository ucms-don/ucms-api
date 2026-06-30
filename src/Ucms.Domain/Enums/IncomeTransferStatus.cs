namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Статус передачи при приходе
/// </summary>
public enum IncomeTransferStatus
{
    /// <summary>
    /// Получено
    /// </summary>
    [Display(Name = "IncomeTransferStatus_Received")]
    Received = 0,

    /// <summary>
    /// Подтверждено
    /// </summary>
    [Display(Name = "IncomeTransferStatus_Approved")]
    Approved = 20,

    /// <summary>
    /// Отклонено
    /// </summary>
    [Display(Name = "IncomeTransferStatus_Cancelled")]
    Cancelled = 99,
}
