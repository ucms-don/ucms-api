namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Kassadan kassaga (bank hisobidan naqd pul kassasiga) o'tkazma.
/// </summary>
public class AccountTransfer : AuditableEntity, IDeletable, IHasOrganization
{
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Qaysi hisob/kassadan (odatda bank hisob)
    /// </summary>
    public Guid FromAccountId { get; set; }

    /// <summary>
    /// Qaysi hisob/kassaga (odatda naqd kassa)
    /// </summary>
    public Guid ToAccountId { get; set; }

    /// <summary>
    /// O'tkaziladigan asosiy summa
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Bank komissiyasi / o'tkazma uchun harajat
    /// </summary>
    public decimal Commission { get; set; }

    /// <summary>
    /// Kim o'tkaz(di — F.I.SH. (Vladalets)
    /// </summary>
    public string TransferredBy { get; set; } = default!;

    /// <summary>
    /// O'tkazma sanasi
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Izoh
    /// </summary>
    public string? Note { get; set; }

    public bool IsDeleted { get; set; }

    public virtual CashAccount? FromAccount { get; set; }
    public virtual CashAccount? ToAccount { get; set; }
    public virtual Organization? Organization { get; set; }
}
