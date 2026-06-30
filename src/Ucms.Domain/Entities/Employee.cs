namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Tashkilot xodimi
/// Сотрудник организации
/// </summary>
public class Employee : AuditableEntity, IDeletable
{
    /// <summary>
    /// Tashkilot ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Xodim to'liq ismi
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Lavozimi
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Izoh / qo'shimcha ma'lumot
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Bog'liq tizim foydalanuvchisi ID (ixtiyoriy)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Tegishli brigada ID (ixtiyoriy)
    /// </summary>
    public Guid? BrigadeId { get; set; }

    /// <summary>
    /// Faol yoki yo'q
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// O'chirilgan yoki yo'q
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Organization? Organization { get; set; }
    public virtual Brigade? Brigade { get; set; }
    public virtual ICollection<Salary> Salaries { get; set; } = [];
}
