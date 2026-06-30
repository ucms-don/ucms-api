namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Статус передача потребности склада
/// </summary>
public enum StockDemandBroadcastStatus
{
    /// <summary>
    /// По умолчанию
    /// </summary>
    [Display(Name = "StockDemandBroadcastStatus_Default")]
    Default = 0,

    /// <summary>
    /// Отправлено
    /// </summary>
    [Display(Name = "StockDemandBroadcastStatus_Sent")]
    Sent = 10,

    /// <summary>
    /// Подтверждено
    /// </summary>
    [Display(Name = "StockDemandBroadcastStatus_Approved")]
    Approved = 20,

    /// <summary>
    /// Отклонено
    /// </summary>
    [Display(Name = "StockDemandBroadcastStatus_Cancelled")]
    Cancelled = 99,
}
