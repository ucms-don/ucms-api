namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Единица складского учета склада
/// </summary>
public class StockBalanceRegister : Entity, IDeletable
{
    /// <summary>
    /// Идентификатор склада
    /// </summary>
    public required Guid StockId { get; set; }

    /// <summary>
    /// Идентификатор единицы складского учета
    /// </summary>
    public required Guid SkuId { get; set; }

    /// <summary>
    /// Идентификатор продукта
    /// </summary>
    public required Guid ProductId { get; set; }

    /// <summary>
    /// Идентификатор единицы измерение
    /// </summary>
    public Guid MeasurementUnitId { get; set; }

    /// <summary>
    /// Предыдущее количество
    /// </summary>
    public decimal PreviousAmount { get; set; }

    /// <summary>
    /// Текущее количество
    /// </summary>
    public decimal CurrentAmount { get; set; }

    /// <summary>
    /// Переменное количество
    /// </summary>
    public decimal VariableAmount { get; set; }

    /// <summary>
    /// Дата
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Тип записа
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }


    public virtual Sku? Sku { get; set; }
    public virtual Stock? Stock { get; set; }
    public virtual Product? Product { get; set; }
    public virtual MeasurementUnit? MeasurementUnit { get; set; }
}
