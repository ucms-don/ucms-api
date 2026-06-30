namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Элемент прихода
/// </summary>
public class IncomeItem : Entity, IDeletable
{
    /// <summary>
    /// Идентификатор прихода
    /// </summary>
    public Guid IncomeId { get; set; }

    /// <summary>
    /// Идентификатор единицы складского учета
    /// </summary>
    public Guid SkuId { get; set; }

    /// <summary>
    /// Идентификатор единицы измерение
    /// </summary>
    public Guid? MeasurementUnitId { get; set; }

    /// <summary>
    /// Количество прихода
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }


    public virtual Sku? Sku { get; set; }
    public virtual Income? Outcome { get; set; }
    public virtual MeasurementUnit? MeasurementUnit { get; set; }
}
