namespace Ucms.Application.Features.Outcomes.DTOs;

using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Domain.Enums;

public record OutcomeModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Note { get; set; }
    public OutcomeType OutcomeType { get; set; }
    public OutcomeStatus OutcomeStatus { get; set; }
    public OutcomeTransferStatus? OutcomeTransferStatus { get; set; }
    public PaymentType PaymentType { get; set; }
    public DateTimeOffset OutcomeDate { get; set; }
    public Guid StockId { get; set; }
    public Guid? IncomeStockId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? ExecutionId { get; set; }
    public StockModel? Stock { get; set; }
    public StockModel? IncomeStock { get; set; }
    public string? FilePath { get; set; }
    public List<OutcomeItemModel> OutcomeItems { get; set; } = [];
}
