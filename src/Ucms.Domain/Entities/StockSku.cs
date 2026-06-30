namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Единица складского учета склада
/// </summary>
public class StockSku : Entity, IDeletable
{
    /// <summary>
    /// Идентификатор склада
    /// </summary>
    public Guid StockId { get; set; } = default!;

    /// <summary>
    /// Идентификатор единицы складского учета
    /// </summary>
    public Guid SkuId { get; set; } = default!;

    /// <summary>
    /// Идентификатор единицы измерение
    /// </summary>
    public Guid? MeasurementUnitId { get; set; }

    /// <summary>
    /// Место нахождение
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Текущее количество
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }


    public virtual Sku? Sku { get; set; }
    public virtual Stock? Stock { get; set; }
    public virtual MeasurementUnit? MeasurementUnit { get; set; }
}
