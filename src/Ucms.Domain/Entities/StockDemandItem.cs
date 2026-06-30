namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Элементы потробности
/// </summary>
public class StockDemandItem : Entity, IDeletable
{

    /// <summary>
    /// Идентификатор потребности
    /// </summary>
    public Guid StockDemandId { get; set; }

    /// <summary>
    /// Идентификатор продукта
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Количество
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Идентификатор единицы измерение
    /// </summary>
    public Guid MeasurementUnitId { get; set; }

    /// <summary>
    /// Примечание к элементу
    /// </summary>
    public string? Note { get; set; }

    public virtual MeasurementUnit? MeasurementUnit { get; set; }

    public virtual StockDemand? StockDemand { get; set; }

    public virtual Product? Product { get; set; }

    /// <summary>
    /// Не подтвержден
    /// </summary>
    public bool NotApproved { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }
}
