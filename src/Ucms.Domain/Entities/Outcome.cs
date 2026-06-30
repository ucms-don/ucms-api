namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Расход
/// </summary>
public class Outcome : Entity, IDeletable
{
    /// <summary>
    /// Наименовние расхода
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Примичание
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Дата расхода
    /// </summary>
    public DateTimeOffset OutcomeDate { get; set; }

    /// <summary>
    /// Тип расхода
    /// </summary>
    public OutcomeType OutcomeType { get; set; }

    /// <summary>
    /// Статус расхода
    /// </summary>
    public OutcomeStatus OutcomeStatus { get; set; }

    /// <summary>
    /// Статус передачи
    /// </summary>
    public OutcomeTransferStatus? OutcomeTransferStatus { get; set; }

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

    public Guid? ExecutionId { get; set; }

    public virtual Stock? Stock { get; set; }
    public IncomeOutcome? IncomeOutcome { get; set; }
    public virtual ICollection<OutcomeItem> OutcomeItems { get; set; } = [];
}
