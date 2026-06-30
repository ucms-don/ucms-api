namespace Ucms.Application.Features.StockSkus.DTOs;

using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Features.Stocks.DTOs;

public record StockSkuModel
{
    public decimal Amount { get; set; }

    public SkuModel? Sku { get; set; }
    public StockModel? Stock { get; set; }
    public MeasurementUnitModel? MeasurementUnit { get; set; }
}
