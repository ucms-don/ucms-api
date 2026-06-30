namespace Ucms.Application.Features.ClientActs.DTOs;

using Ucms.Domain.Enums;

public record ClientActDetailDto(
    Guid                          Id,
    string                        ActNumber,
    DateTimeOffset                ActDate,
    decimal                       TotalAmount,
    ActStatus                     Status,
    string?                       Note,
    DateTimeOffset                CreatedAt,
    DateTimeOffset                UpdatedAt,
    IEnumerable<ClientActItemDto>    Items,
    IEnumerable<ClientActPaymentDto> Payments,
    decimal                       PaidAmount);
