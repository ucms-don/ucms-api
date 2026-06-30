namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Loyiha smetasi (bir loyihada bir nechta smeta bo'lishi mumkin)
/// </summary>
public class Estimate : AuditableEntity, IDeletable
{
    /// <summary>
    /// Loyiha ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Smeta nomi (e.g. "Asosiy smeta", "Qo'shimcha ishlar")
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Izoh / tavsif
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tartib raqami
    /// </summary>
    public int Order { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Project? Project { get; set; }
    public virtual ICollection<EstimateSection> Sections { get; set; } = [];
}
