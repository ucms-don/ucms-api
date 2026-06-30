namespace Ucms.Application.Features.StockSkus.DTOs;

using Ucms.Domain.Enums;

public record StockInventoryModel
{
    public StockInventoryDataModel? Data { get; set; }
    public decimal Amount { get; set; }
}

public record StockInventoryDataModel
{
    public Guid ProductId { get; set; }
    public Guid? MeasurementUnitId { get; set; }
    public decimal Amount { get; set; }
    public string SkuName { get; set; } = default!;
    public string SkuNameRu { get; set; } = default!;
    public string? SkuNameEn { get; set; }
    public string? SkuNameKa { get; set; }
    public string MeasurementUnitName { get; set; } = default!;
    public string MeasurementUnitNameRu { get; set; } = default!;
    public string? MeasurementUnitNameEn { get; set; }
    public string? MeasurementUnitNameKa { get; set; }
    public MeasurementUnitType MeasurementUnitType { get; set; }
}
