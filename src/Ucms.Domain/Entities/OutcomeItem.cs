namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Элемент расхода
/// </summary>
public class OutcomeItem : Entity, IDeletable
{
    /// <summary>
    /// Идентификатор расхода
    /// </summary>
    public Guid OutcomeId { get; set; }

    /// <summary>
    /// Идентификатор единицы складского учета
    /// </summary>
    public Guid SkuId { get; set; }

    /// <summary>
    /// Идентификатор единицы измерение
    /// </summary>
    public Guid? MeasurementUnitId { get; set; }

    /// <summary>
    /// Количество расхода
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// фактический расход
    /// </summary>
    public decimal ActualAmount { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }


    public virtual Sku? Sku { get; set; }
    public virtual Outcome? Outcome { get; set; }
    public virtual MeasurementUnit? MeasurementUnit { get; set; }

}
