namespace Ucms.Application.Features.StockDemands.DTOs;

using Ucms.Domain.Enums;

public record StockDemandModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Note { get; set; }
    public DateTimeOffset DemandDate { get; set; }
    public StockDemandStatus DemandStatus { get; set; }
    public StockDemandBroadcastStatus BroadcastStatus { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? OutcomeId { get; set; }
    public IEnumerable<StockDemandItemModel>? Items { get; set; }
}
