namespace Ucms.Application.Features.WorkLogs.DTOs;

using Ucms.Domain.Enums;

public record WorkLogDetailDto(
    Guid                         Id,
    DateTimeOffset               Date,
    decimal                      Volume,
    decimal                      BrigadeUnitPrice,
    decimal                      TotalAmount,
    WorkLogStatus                Status,
    string?                      Floor,
    string?                      Zone,
    string?                      Room,
    string?                      Note,
    Guid?                        BrigadePaymentId,
    DateTimeOffset               CreatedAt,
    DateTimeOffset               UpdatedAt,
    WorkLogBrigadeDto            Brigade,
    WorkLogDetailEstimateItemDto EstimateItem);
