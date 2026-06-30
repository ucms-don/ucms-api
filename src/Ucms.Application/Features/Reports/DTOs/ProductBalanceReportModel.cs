namespace Ucms.Application.Features.Reports.DTOs;

using Ucms.Domain.Enums;

public class ProductBalanceReportModel
{
    public ProductBalanceReportModel()
    {
        ProductTypes = [];
    }

    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public Guid OrganizationId { get; set; }
    public List<ProductBalanceReportProductTypeModel> ProductTypes { get; set; }
}

public class ProductBalanceReportProductTypeModel
{
    public ProductBalanceReportProductTypeModel()
    {
        Products = [];
    }

    public ProductType ProductType { get; set; }
    public List<ProductBalanceReportProductModel> Products { get; set; }
}

public class ProductBalanceReportProductModel
{
    public ProductBalanceReportProductModel()
    {
        Skus = [];
    }

    public string ProductName { get; set; } = default!;
    public string ProductNameRu { get; set; } = default!;
    public string? ProductNameEn { get; set; }
    public string? ProductNameKa { get; set; }
    public string MeasurementUnitName { get; set; } = default!;
    public string MeasurementUnitNameRu { get; set; } = default!;
    public string? MeasurementUnitNameEn { get; set; }
    public string? MeasurementUnitNameKa { get; set; }
    public MeasurementUnitType MeasurementUnitType { get; set; }
    public List<ProductBalanceReportSkuModel> Skus { get; set; }
}

public class ProductBalanceReportSkuModel
{
    public string Seria { get; set; } = default!;
    public DateTimeOffset ExpirationDate { get; set; }

    public decimal CentralStockFromBalance { get; set; }
    public decimal ChildStocksFromBalance { get; set; }
    public decimal AllStocksFromBalance => CentralStockFromBalance + ChildStocksFromBalance;

    public decimal CentralStockIncome { get; set; }
    public decimal CentralStockBroadcastOutcome { get; set; }
    public decimal AllStocksUsageOutcome { get; set; }

    public decimal CentralStockToBalance { get; set; }
    public decimal ChildStocksToBalance { get; set; }
    public decimal AllStocksToBalance => CentralStockToBalance + ChildStocksToBalance;

}
