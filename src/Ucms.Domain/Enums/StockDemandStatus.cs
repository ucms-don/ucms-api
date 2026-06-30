namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Статус потребности склада
/// </summary>
public enum StockDemandStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    [Display(Name = "StockDemandStatus_Draft")]
    Draft = 0,

    /// <summary>
    /// Отправлено
    /// </summary>
    [Display(Name = "StockDemandStatus_Sent")]
    Sent = 10,

    /// <summary>
    /// Подтверждено
    /// </summary>
    [Display(Name = "StockDemandStatus_Approved")]
    Approved = 20,

    /// <summary>
    /// Отклонено
    /// </summary>
    [Display(Name = "StockDemandStatus_Cancelled")]
    Cancelled = 99,
}
