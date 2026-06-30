namespace Ucms.Application.Features.Outcomes.DTOs;

public record CreateOutcomeItemModel(
    Guid SkuId,
    Guid MeasurementUnitId,
    decimal Amount,
    decimal ActualAmount
);
