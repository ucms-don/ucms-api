namespace Ucms.Application.Features.Projects.DTOs;

public record ProjectEstimateItemDto(
    Guid    Id,
    string  Name,
    string  Unit,
    decimal Volume,
    decimal ClientUnitPrice,
    decimal BrigadeUnitPrice,
    decimal ClientTotal,
    decimal BrigadeTotal,
    int     Order);
