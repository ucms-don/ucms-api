namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Склад
/// </summary>
public class Stock : LocalizableEntity, IDeletable
{
    /// <summary>
    /// Идентификатор организации (с Organization Service)
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Родительский склад
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Код склада
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Условие хранение
    /// </summary>
    public StorageCondition StorageCondition { get; set; }

    /// <summary>
    /// Тип склада
    /// </summary>
    public StockType StockType { get; set; }

    /// <summary>
    /// Категория склада
    /// </summary>
    public StockCategory StockCategory { get; set; }

    /// <summary>
    /// Идентификатор прикрепленных сотрудников
    /// </summary>
    public Guid[] EmployeeIds { get; set; } = [];

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Stock? Parent { get; set; }

    public virtual ICollection<Stock> Childs { get; set; } = [];
}
