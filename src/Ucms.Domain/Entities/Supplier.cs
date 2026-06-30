namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Поставщик
/// </summary>
public class Supplier : LocalizableEntity, IDeletable
{
    /// <summary>
    /// Код поставщика
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }
}
