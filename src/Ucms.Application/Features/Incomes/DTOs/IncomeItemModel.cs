namespace Ucms.Application.Features.Incomes.DTOs;

using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Features.Skus.DTOs;

public record IncomeItemModel(
    Guid Id,
    Guid SkuId,
    decimal Amount,
    Guid? MeasurementUnitId,
    SkuModel? Sku,
    MeasurementUnitModel? MeasurementUnit
);
