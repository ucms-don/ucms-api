namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Расход
/// </summary>
public class IncomeOutcome : Entity, IDeletable
{
    /// <summary>
    /// Дата расхода
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Идентификатор расхода
    /// </summary>
    public Guid? OutcomeId { get; set; }

    /// <summary>
    /// Идентификатор прихода
    /// </summary>
    public Guid? IncomeId { get; set; }

    /// <summary>
    /// Идентификатор передающиеся склада
    /// </summary>
    public Guid OutcomeStockId { get; set; }

    /// <summary>
    /// Идентификатор принимающего склада
    /// </summary>
    public Guid IncomeStockId { get; set; }

    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    public virtual Outcome? Outcome { get; set; }
    public virtual Income? Income { get; set; }
    public virtual Stock? OutcomeStock { get; set; }
    public virtual Stock? IncomeStock { get; set; }
}
