namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Zakazchikdan tushgan to'lov
/// </summary>
public class ClientPayment : AuditableEntity
{
    /// <summary>
    /// Loyiha ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Akt ID (qaysi aktga tegishli)
    /// </summary>
    public Guid? ActId { get; set; }

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
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

    /// <summary>
    /// Izoh / hujjat raqami
    /// </summary>
    public string? Note { get; set; }

    public virtual Project? Project { get; set; }
    public virtual ClientAct? Act { get; set; }
}
