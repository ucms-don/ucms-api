namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Единица складского учета
/// </summary>
public class Sku : Entity, IDeletable
{
    /// <summary>
    /// Сериа номер Единицы складского учета
    /// </summary>
    public string SerialNumber { get; set; } = default!;

    /// <summary>
    /// Срок годности
    /// </summary>
    public DateTimeOffset ExpirationDate { get; set; }

    /// <summary>
    /// Sana — material omborga qabul qilingan sana (standart: yaratilgan vaqt)
    /// </summary>
    public DateTimeOffset PurchaseDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Идентификатор продукта
    /// </summary>
    public required Guid ProductId { get; set; }

    /// <summary>
    /// Идентификатор производителя
    /// </summary>
    public Guid? ManufacturerId { get; set; }

    /// <summary>
    /// Идентификатор единицы измерение
    /// </summary>
    public required Guid MeasurementUnitId { get; set; }

    /// <summary>
    /// Идентификатор поставщика
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Количество Единицы складского учета
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Цена
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Статус
    /// </summary>
    public SkuStatus Status { get; set; } = SkuStatus.Default;

    public virtual Product? Product { get; set; }

    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual MeasurementUnit? MeasurementUnit { get; set; }

    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<StockBalanceRegister> StockBalanceRegistery { get; set; } = [];
}
