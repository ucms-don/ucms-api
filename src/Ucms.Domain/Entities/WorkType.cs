namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Ish turi sprvashnigi (e.g. "Стяжка пола", "Штукатурка стен")
/// </summary>
public class WorkType : LocalizableEntity, IDeletable
{
    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }
}
