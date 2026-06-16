namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Buyurtmachi (zakazchik) — jismoniy yoki yuridik shaxs.
/// Avval Project.ClientName oddiy string edi, endi barqaror identifikator sifatida qo'shildi.
/// </summary>
public class Customer : AuditableEntity, IDeletable, IHasOrganization
{
    /// <summary>
    /// Tashkilot ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Buyurtmachi nomi (jismoniy shaxs F.I.Sh yoki kompaniya nomi)
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// INN / pasport ma'lumoti (ixtiyoriy)
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Manzil
    /// </summary>
    public string? Address { get; set; }

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
    public virtual ICollection<Project> Projects { get; set; } = [];
}
