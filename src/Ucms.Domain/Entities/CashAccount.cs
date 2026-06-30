namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Tashkilotga tegishli kassa yoki bank hisobi.
/// Balans saqlanmaydi — har doim CashTransaction'lardan real vaqtda hisoblanadi.
/// </summary>
public class CashAccount : AuditableEntity, IDeletable, IHasOrganization
{
    /// <summary>
    /// Tashkilot ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Hisob nomi (masalan: "Asosiy kassa", "Ipak Yo'li bank hisobi")
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Hisob turi: Naqd yoki Bank
    /// </summary>
    public CashAccountType Type { get; set; } = CashAccountType.Cash;

    /// <summary>
    /// Izoh
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Faol yoki yo'q
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// O'chirilgan yoki yo'q
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Organization? Organization { get; set; }
    public virtual ICollection<CashTransaction> Transactions { get; set; } = [];
}
