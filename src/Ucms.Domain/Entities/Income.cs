namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Приход
/// </summary>
public class Income : Entity, IDeletable
{
    /// <summary>
    /// Наименование прихода
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Примечание
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Дата прихода
    /// </summary>
    public DateTimeOffset IncomeDate { get; set; }

    /// <summary>
    /// Тип прихода
    /// </summary>
    public IncomeType IncomeType { get; set; }

    /// <summary>
    /// Статус прихода
    /// </summary>
    public IncomeStatus IncomeStatus { get; set; }

    /// <summary>
    /// Статус передачи
    /// </summary>
    public IncomeTransferStatus? IncomeTransferStatus { get; set; }

    /// <summary>
    /// Тип оплаты
    /// </summary>
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// Имя сотрудника
    /// </summary>
    public string? EmployeeName { get; set; }

    /// <summary>
    /// Идентификатор сотрудника
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    public string? FilePath { get; set; }

    /// <summary>
    /// Идентификатор склада
    /// </summary>
    public Guid StockId { get; set; }

    public virtual Stock? Stock { get; set; }

    public IncomeOutcome? IncomeOutcome { get; set; }
    public virtual ICollection<IncomeItem> IncomeItems { get; set; } = [];
}
