namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Qurilish loyihasi
/// </summary>
public class Project : AuditableEntity, IDeletable, IHasOrganization
{
    /// <summary>
    /// Tashkilot ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Loyiha nomi (e.g. "ИКС отделка сек. 2,3")
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Qurilish manzili / obyekt
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Tavsif
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Shartnoma raqami
    /// </summary>
    public string? ContractNumber { get; set; }

    /// <summary>
    /// Shartnoma sanasi
    /// </summary>
    public DateTimeOffset? ContractDate { get; set; }

    /// <summary>
    /// Boshlanish sanasi
    /// </summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Tugash sanasi (rejadagi)
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// Buyurtmachi ID (ixtiyoriy — Customer entity'ga bog'lanish)
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Shartnoma summasi
    /// </summary>
    public decimal? ContractValue { get; set; }

    /// <summary>
    /// Loyiha holati
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    /// <summary>
    /// O'chirilgan yoki yo'q
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Organization? Organization { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<Estimate> Estimates { get; set; } = [];
    public virtual ICollection<WorkLog> WorkLogs { get; set; } = [];
    public virtual ICollection<ClientAct> ClientActs { get; set; } = [];
    public virtual ICollection<ClientPayment> ClientPayments { get; set; } = [];
    public virtual ICollection<BrigadePayment> BrigadePayments { get; set; } = [];
    public virtual ICollection<ProjectExpense> Expenses { get; set; } = [];
}
