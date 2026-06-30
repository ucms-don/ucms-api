namespace Ucms.Application.Features.StockSkus.DTOs;

public record StockSkuStatModel(
    decimal CarStockSkuAmount,
    decimal CaseStockSkuAmount,
    decimal OtherStockSkuAmount
);
