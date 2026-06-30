namespace Ucms.Application.Features.Incomes.DTOs;

using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Domain.Enums;

public record IncomeModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Note { get; set; }
    public IncomeType IncomeType { get; set; }
    public IncomeStatus IncomeStatus { get; set; }
    public IncomeTransferStatus? IncomeTransferStatus { get; set; }
    public PaymentType PaymentType { get; set; }
    public DateTimeOffset IncomeDate { get; set; }
    public Guid StockId { get; set; }
    public Guid? OutcomeStockId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? EmployeeId { get; set; }
    public StockModel? Stock { get; set; }
    public StockModel? OutcomeStock { get; set; }
    public string? FilePath { get; set; }
    public IEnumerable<IncomeItemModel> IncomeItems { get; set; } = [];
};
