namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;
using Ucms.Domain.Enums;

/// <summary>
/// Потребности склада
/// </summary>
public class StockDemand : Entity, IDeletable
{
    /// <summary>
    /// Наименование документа
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Примечание документа
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Идентификатор склада отправителя потребности
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Идентификатор склада получателя потребности
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Дата запроса
    /// </summary>
    public DateTimeOffset DemandDate { get; set; }

    /// <summary>
    /// Статус запроса
    /// </summary>
    public StockDemandStatus DemandStatus { get; set; }

    /// <summary>
    /// Статус передача
    /// </summary>
    public StockDemandBroadcastStatus BroadcastStatus { get; set; }

    /// <summary>
    /// Имя сотрудника
    /// </summary>
    public string? EmployeeName { get; set; }

    /// <summary>
    /// Идентификатор сотрудника
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Идентификатор расхода
    /// </summary>
    public Guid? OutcomeId { get; set; }

    public virtual ICollection<StockDemandItem> StockDemandItems { get; set; } = [];

    public virtual Stock? Sender { get; set; }
    public virtual Stock? Recipient { get; set; }
    public virtual Outcome? Outcome { get; set; }
}
