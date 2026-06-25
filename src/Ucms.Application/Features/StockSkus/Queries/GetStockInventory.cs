namespace Ucms.Application.Features.StockSkus.Queries;

using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.StockSkus.DTOs;
using Ucms.Application.Persistence;

public static class GetStockInventory
{
    public record Query(PagedRequest Paging, Guid? StockId, Guid? ProductId, Guid? OrganizationId);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext)
    {
        public async Task<PagedResult<StockInventoryModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.StockSkus.AsQueryable();
            if (q.OrganizationId.HasValue) query = query.Where(w => w.Stock!.OrganizationId == q.OrganizationId);
            if (q.StockId.HasValue) query = query.Where(w => w.StockId == q.StockId);
            if (q.ProductId.HasValue) query = query.Where(w => w.Sku!.ProductId == q.ProductId);

            var result = await query.OrderBy(o => o.Sku!.Product!.NameRu)
                .Select(s => new StockInventoryDataModel
                {
                    Amount = s.Amount, ProductId = s.Sku!.ProductId, MeasurementUnitId = s.MeasurementUnitId,
                    SkuName = s.Sku!.Product!.Name, SkuNameEn = s.Sku!.Product!.NameEn,
                    SkuNameRu = s.Sku!.Product!.NameRu, SkuNameKa = s.Sku!.Product!.NameKa,
                    MeasurementUnitName = s.MeasurementUnit!.Name, MeasurementUnitNameEn = s.MeasurementUnit!.NameEn,
                    MeasurementUnitNameRu = s.MeasurementUnit!.NameRu, MeasurementUnitNameKa = s.MeasurementUnit!.NameKa,
                    MeasurementUnitType = s.MeasurementUnit!.Type
                })
                .GroupBy(a => new { a.ProductId, a.MeasurementUnitId })
                .Select(s => new StockInventoryModel { Data = s.First(), Amount = s.Sum(ss => ss.Amount) })
                .ToPagedResultAsync(q.Paging, ct);

            var muTypes = result.Items.Select(s => s.Data!.MeasurementUnitType);
            var mus = await db.OrganizationMeasurementUnits
                .Where(w => w.OrganizationId == workContext.TenantId && muTypes.Contains(w.Type))
                .Select(s => s.MeasurementUnit).ToListAsync(ct);

            result.Items.ForEach(item =>
            {
                if (item.Data is null) return;
                var mu = mus.FirstOrDefault(f => f!.Type == item.Data.MeasurementUnitType);
                if (mu is null) return;
                item.Data.MeasurementUnitId = mu.Id; item.Data.MeasurementUnitName = mu.Name;
                item.Data.MeasurementUnitNameEn = mu.NameEn; item.Data.MeasurementUnitNameRu = mu.NameRu;
                item.Data.MeasurementUnitNameKa = mu.NameKa; item.Data.MeasurementUnitType = mu.Type;
                item.Amount /= mu.Multiplier > 0 ? mu.Multiplier : 1;
            });
            return result;
        }
    }
}
