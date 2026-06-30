namespace Ucms.Application.Features.ClientActs.DTOs;

using Ucms.Domain.Enums;

public record ClientActPaymentDto(
    Guid            Id,
    DateTimeOffset  Date,
    decimal         Amount,
    PaymentMethod   PaymentMethod,
    string?         Note);
