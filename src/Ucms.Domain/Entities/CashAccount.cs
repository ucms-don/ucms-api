namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Tashkilotga tegishli kassa yoki bank hisobi.
/// Balans denormalizatsiya qilingan: har bir moliyaviy yozuvda apply_cash_balance_delta()
/// SP orqali FOR UPDATE lock bilan sinxron yangilanadi.
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
    /// Joriy balans (denormalizatsiya). Faqat apply_cash_balance_delta() SP orqali o'zgartiriladi.
    /// </summary>
    public decimal Balance { get; set; } = 0;

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
