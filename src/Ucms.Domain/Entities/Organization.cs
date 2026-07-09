namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Tashkilot — tizim egasi (Owner) yoki foydalanuvchi kompaniya (Tenant)
/// </summary>
public class Organization : AuditableEntity, IDeletable
{
    /// <summary>
    /// Tashkilot turi: Owner = tizim egasi, Tenant = foydalanuvchi
    /// </summary>
    public OrganizationType Type { get; set; } = OrganizationType.Tenant;

    /// <summary>
    /// Tashkilot nomi
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// INN (soliq raqami)
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Manzil
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Elektron pochta
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Test tashkiloti belgisi — faqat Owner tashkilot tomonidan o'rnatiladi
    /// </summary>
    public bool IsTest { get; set; }

    /// <summary>
    /// Ota-tashkilot ID (ierarxiya uchun, null = ildiz)
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// O'chirilgan yoki yo'q
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Organization?             Parent   { get; set; }
    public virtual ICollection<Organization> Children { get; set; } = [];
    public virtual ICollection<Project>      Projects { get; set; } = [];
    public virtual ICollection<Brigade>      Brigades { get; set; } = [];
}
