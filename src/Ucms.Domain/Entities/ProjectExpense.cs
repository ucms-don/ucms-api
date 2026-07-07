namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Loyiha xarajati — materiallar, transport, boshqa chiqimlar
/// Расход по проекту — материалы, транспорт, прочие затраты
/// </summary>
public class ProjectExpense : CancellableEntity, IDeletable
{
    /// <summary>
    /// Tashkilot ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Loyiha ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Xarajat sanasi
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Kategoriya (Materiallar, Transport, Ijara, Maosh, Boshqa)
    /// </summary>
    public string Category { get; set; } = default!;

    /// <summary>
    /// Summa
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tavsif
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// To'lov usuli (Naqd, Karta, Bank o'tkazma)
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Izoh
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// O'chirilgan yoki yo'q
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Project? Project { get; set; }
}
