namespace Ucms.Application.Features.Reports.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Reports.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class GetProductBalanceReport
{
    public record Query(DateTime From, DateTime To, Guid OrganizationId);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext)
    {
        public async Task<ProductBalanceReportModel> HandleAsync(Query q, CancellationToken ct)
        {
            var skuIds = await db.OrganizationSkus
                .Where(w => w.OrganizationId == q.OrganizationId)
                .Select(s => s.SkuId).ToListAsync(ct);

            var records = await db.StockBalanceRegisters
                .Include(i => i.Sku).Include(i => i.Product)
                .Include(i => i.MeasurementUnit).Include(i => i.Stock)
                .Where(w => w.Date >= q.From && w.Date <= q.To && skuIds.Contains(w.SkuId))
                .OrderByDescending(w => w.Date).ToListAsync(ct);

            var result = BuildReport(q, records);
            await ApplyOrgMeasurementUnits(result, ct);
            return result;
        }

        private static ProductBalanceReportModel BuildReport(Query q, List<StockBalanceRegister> records)
        {
            return new ProductBalanceReportModel
            {
                From = q.From,
                To = q.To,
                OrganizationId = q.OrganizationId,
                ProductTypes = [.. records.GroupBy(g => g.Product!.Type).Select(s => new ProductBalanceReportProductTypeModel
                {
                    ProductType = s.Key,
                    Products = [.. s.GroupBy(g => g.ProductId).Select(ss => new ProductBalanceReportProductModel
                    {
                        ProductName = ss.First().Product!.Name,
                        ProductNameRu = ss.First().Product!.NameRu,
                        ProductNameEn = ss.First().Product!.NameEn,
                        ProductNameKa = ss.First().Product!.NameKa,
                        MeasurementUnitName = ss.First().MeasurementUnit!.Name,
                        MeasurementUnitNameRu = ss.First().MeasurementUnit!.NameRu,
                        MeasurementUnitNameEn = ss.First().MeasurementUnit!.NameEn,
                        MeasurementUnitNameKa = ss.First().MeasurementUnit!.NameKa,
                        MeasurementUnitType = ss.First().MeasurementUnit!.Type,
                        Skus = [.. ss.GroupBy(g => g.Sku!.SerialNumber).Select(sss => new ProductBalanceReportSkuModel
                        {
                            Seria = sss.Key,
                            ExpirationDate = sss.First().Sku!.ExpirationDate,
                            CentralStockFromBalance = GetFromBalance(sss, StockCategory.Central, q.From),
                            ChildStocksFromBalance = GetFromBalance(sss, StockCategory.Default, q.From),
                            CentralStockIncome = sss.Where(w => w.Stock!.StockCategory == StockCategory.Central && w.VariableAmount > 0).Sum(s => s.VariableAmount),
                            CentralStockBroadcastOutcome = sss.Where(w => w.Stock!.StockCategory == StockCategory.Central && w.VariableAmount < 0).Sum(s => Math.Abs(s.VariableAmount)),
                            AllStocksUsageOutcome = sss.Where(w => w.VariableAmount < 0 && w.Type == (int)OutcomeType.Usage).Sum(s => Math.Abs(s.VariableAmount)),
                            CentralStockToBalance = GetToBalance(sss, StockCategory.Central),
                            ChildStocksToBalance = GetToBalance(sss, StockCategory.Default),
                        })]
                    })]
                })]
            };
        }

        private static decimal GetFromBalance(IGrouping<string, StockBalanceRegister> g, StockCategory cat, DateTime from)
        {
            var rec = g.LastOrDefault(l => l.Stock!.StockCategory == cat);
            if (rec is null)
                return 0;
            return rec.Date.Date == from.Date ? rec.CurrentAmount : rec.PreviousAmount;
        }

        private static decimal GetToBalance(IGrouping<string, StockBalanceRegister> g, StockCategory cat)
        {
            return g.FirstOrDefault(l => l.Stock!.StockCategory == cat)?.CurrentAmount ?? 0;
        }

        private async Task ApplyOrgMeasurementUnits(ProductBalanceReportModel report, CancellationToken ct)
        {
            var mus = await db.OrganizationMeasurementUnits
                .Where(w => w.OrganizationId == workContext.TenantId)
                .Select(s => s.MeasurementUnit).ToListAsync(ct);

            foreach (var pt in report.ProductTypes)
                foreach (var product in pt.Products)
                {
                    var mu = mus.FirstOrDefault(f => f!.Type == product.MeasurementUnitType);
                    if (mu is null)
                        continue;
                    var mul = mu.Multiplier > 0 ? mu.Multiplier : 1;
                    product.MeasurementUnitName = mu.Name;
                    product.MeasurementUnitNameRu = mu.NameRu;
                    product.MeasurementUnitNameEn = mu.NameEn;
                    product.MeasurementUnitNameKa = mu.NameKa;
                    foreach (var sku in product.Skus)
                    {
                        sku.CentralStockFromBalance /= mul;
                        sku.ChildStocksFromBalance /= mul;
                        sku.CentralStockIncome /= mul;
                        sku.CentralStockBroadcastOutcome /= mul;
                        sku.AllStocksUsageOutcome /= mul;
                        sku.CentralStockToBalance /= mul;
                        sku.ChildStocksToBalance /= mul;
                    }
                }
        }
    }
}
