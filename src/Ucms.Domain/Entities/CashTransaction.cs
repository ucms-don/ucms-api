namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Yangi Finance modulidagi yagona pul harakati yozuvi.
/// Diqqat: bu eski ClientPayment/BrigadePayment/Salary/ProjectExpense'ni almashtirmaydi —
/// ular o'zgarishsiz qoladi (ADR-0001, Option A'). CashTransaction faqat hozircha alohida
/// entity'si yo'q operatsiyalar uchun: Supplier to'lovi, Loan, Owner investitsiyasi.
/// </summary>
public class CashTransaction : AuditableEntity, IDeletable, IHasOrganization
{
    /// <summary>
    /// Tashkilot ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Qaysi kassa/hisob orqali amalga oshirilgani
    /// </summary>
    public Guid CashAccountId { get; set; }

    /// <summary>
    /// Yo'nalish: kirim yoki chiqim
    /// </summary>
    public CashDirection Direction { get; set; }

    /// <summary>
    /// Operatsiya turi
    /// </summary>
    public CashTransactionType TransactionType { get; set; }

    /// <summary>
    /// Partner turi (Supplier, Owner, Lender va h.k.)
    /// </summary>
    public FinancePartnerType PartnerType { get; set; }

    /// <summary>
    /// Partner ID — masalan SupplierId. Owner investitsiyasi/umumiy kredit uchun bo'sh qoldirilishi mumkin.
    /// </summary>
    public Guid? PartnerId { get; set; }

    /// <summary>
    /// Summa (har doim musbat — yo'nalish Direction maydonida)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Operatsiya sanasi
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Bog'liq loyiha (ixtiyoriy — masalan loyihaga tegishli material uchun supplier to'lovi)
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Izoh / hujjat raqami
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// O'chirilgan yoki yo'q
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Organization? Organization { get; set; }
    public virtual CashAccount? CashAccount { get; set; }
    public virtual Project? Project { get; set; }
}
