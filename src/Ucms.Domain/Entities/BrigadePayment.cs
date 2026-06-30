namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Brigadaga to'langan to'lov
/// </summary>
public class BrigadePayment : AuditableEntity
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
    /// To'lov sanasi
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// To'lov summasi
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// To'lov usuli
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>
    /// Izoh
    /// </summary>
    public string? Note { get; set; }

    public virtual Project? Project { get; set; }
    public virtual Brigade? Brigade { get; set; }

    /// <summary>
    /// Ushbu to'lov qoplaydigan ish jurnallari
    /// </summary>
    public virtual ICollection<WorkLog> WorkLogs { get; set; } = [];
}
