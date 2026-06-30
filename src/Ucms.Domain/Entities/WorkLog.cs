namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Bajarilgan ish jurnali — brigada qilgan ish miqdori
/// </summary>
public class WorkLog : AuditableEntity
{
    /// <summary>
    /// Loyiha ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Brigada ID
    /// </summary>
    public Guid BrigadeId { get; set; }

    /// <summary>
    /// Smeta qatori ID
    /// </summary>
    public Guid EstimateItemId { get; set; }

    /// <summary>
    /// Bajarilgan sana
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Bajarilgan hajm
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// Brigada uchun birlik narhi (o'sha vaqtdagi qiymat)
    /// </summary>
    public decimal BrigadeUnitPrice { get; set; }

    /// <summary>
    /// Jami summa = Volume * BrigadeUnitPrice
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Qavat
    /// </summary>
    public string? Floor { get; set; }

    /// <summary>
    /// Zona / blok
    /// </summary>
    public string? Zone { get; set; }

    /// <summary>
    /// Xona / bo'lim
    /// </summary>
    public string? Room { get; set; }

    /// <summary>
    /// Izoh
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Holati
    /// </summary>
    public WorkLogStatus Status { get; set; } = WorkLogStatus.Draft;

    /// <summary>
    /// Brigada to'lovi ID (agar to'langan bo'lsa)
    /// </summary>
    public Guid? BrigadePaymentId { get; set; }

    public virtual Project? Project { get; set; }
    public virtual Brigade? Brigade { get; set; }
    public virtual EstimateItem? EstimateItem { get; set; }
    public virtual BrigadePayment? BrigadePayment { get; set; }
}
