namespace Ucms.Application.Features.ClientActs.DTOs;

public record ClientActItemDto(
    Guid   Id,
    Guid   EstimateItemId,
    string ItemName,
    string Unit,
    decimal Volume,
    decimal UnitPrice,
    decimal TotalAmount);
