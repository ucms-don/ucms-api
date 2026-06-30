namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Akt qatori — qaysi ish uchun, qancha miqdorga
/// </summary>
public class ClientActItem : Entity
{
    /// <summary>
    /// Akt ID
    /// </summary>
    public Guid ActId { get; set; }

    /// <summary>
    /// Smeta qatori ID
    /// </summary>
    public Guid EstimateItemId { get; set; }

    /// <summary>
    /// Hajm (akt bo'yicha qabul qilingan miqdor)
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// Birlik narhi (zakazchik bilan kelishilgan)
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Jami summa = Volume * UnitPrice
    /// </summary>
    public decimal TotalAmount { get; set; }

    public virtual ClientAct? Act { get; set; }
    public virtual EstimateItem? EstimateItem { get; set; }
}
