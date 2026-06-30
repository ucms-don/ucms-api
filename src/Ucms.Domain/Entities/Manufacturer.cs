namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Справочник производителей
/// </summary>
public class Manufacturer : LocalizableEntity, IDeletable
{
    /// <summary>
    /// Код производителя
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }
}
