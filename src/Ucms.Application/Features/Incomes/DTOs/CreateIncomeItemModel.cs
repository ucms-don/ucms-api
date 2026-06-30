namespace Ucms.Application.Features.Incomes.DTOs;

public record CreateIncomeItemModel(
    Guid SkuId,
    Guid MeasurementUnitId,
    decimal Amount
);
