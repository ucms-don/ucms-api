namespace Ucms.Application.Features.StockDemands.DTOs;

using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Features.Products.DTOs;

public record StockDemandItemModel(
    Guid Id,
    Guid ProductId,
    Guid StockDemandId,
    Guid MeasurementUnitId,
    decimal Amount,
    string? Note,
    bool NotApproved,
    ProductModel? Product,
    MeasurementUnitModel? MeasurementUnit
);
