namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Zakazchiga beriladigan ish qabul qilish akti
/// </summary>
public class ClientAct : AuditableEntity
{
    /// <summary>
    /// Loyiha ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Akt raqami
    /// </summary>
    public string ActNumber { get; set; } = default!;

    /// <summary>
    /// Akt sanasi
    /// </summary>
    public DateTimeOffset ActDate { get; set; }

    /// <summary>
    /// Akt bo'yicha jami summa
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Holati
    /// </summary>
    public ActStatus Status { get; set; } = ActStatus.Draft;

    /// <summary>
    /// Izoh
    /// </summary>
    public string? Note { get; set; }

    public virtual Project? Project { get; set; }
    public virtual ICollection<ClientActItem> Items { get; set; } = [];
    public virtual ICollection<ClientPayment> Payments { get; set; } = [];
}
