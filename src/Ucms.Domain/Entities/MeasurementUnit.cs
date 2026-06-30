namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Единица измерение
/// </summary>
public class MeasurementUnit : LocalizableEntity, IDeletable
{
    /// <summary>
    /// Код единицы измерении
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Множитель
    /// </summary>
    public decimal Multiplier { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Тип единицы измерение
    /// </summary>
    public MeasurementUnitType Type { get; set; }

}
