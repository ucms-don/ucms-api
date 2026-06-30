namespace Ucms.Application.Features.Dashboard.DTOs;

using Ucms.Domain.Enums;

public record DashboardPaymentDto(
    Guid          Id,
    DateTimeOffset Date,
    decimal        Amount,
    PaymentMethod  PaymentMethod,
    string         Project,
    string?        ActNumber);
