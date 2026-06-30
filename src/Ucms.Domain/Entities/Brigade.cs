namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Ishchi brigada
/// </summary>
public class Brigade : AuditableEntity, IDeletable
{
    /// <summary>
    /// Tashkilot ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Brigada nomi
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Brigada boshlig'i ismi
    /// </summary>
    public string? ForemanName { get; set; }

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Izoh / qo'shimcha ma'lumot
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
    public virtual ICollection<Employee> Employees { get; set; } = [];
    public virtual ICollection<WorkLog> WorkLogs { get; set; } = [];
    public virtual ICollection<BrigadePayment> Payments { get; set; } = [];
}
