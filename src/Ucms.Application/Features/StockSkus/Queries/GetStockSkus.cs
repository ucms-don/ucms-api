namespace Ucms.Application.Features.StockSkus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Authorization;
using Ucms.Application.Abstractions.Constants;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Application.Features.StockSkus.DTOs;
using Ucms.Application.Features.MeasurementUnits.DTOs;

public static class GetStockSkus
{
    public record Query(Guid OrganizationId, PagedRequest Paging, Guid? StockId, Guid? MeasurementUnitId, Guid? ProductId, Guid? ManufacturerId, string? Seria);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext, IPermissionProvider permissionProvider)
    {
        public async Task<PagedResult<StockSkuModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.StockSkus
                .Include(i => i.Sku!.Manufacturer).Include(i => i.Sku!.Product)
                .Include(i => i.Stock).Include(i => i.MeasurementUnit)
                .Where(s => s.Stock!.OrganizationId == q.OrganizationId);

            if (workContext.TenantId == q.OrganizationId &&
                !await permissionProvider.HasPermissionAsync(Permissions.Warehouse.AccessSettingMinimumBalanceWarehouse, ct))
                query = query.Where(w => w.Stock!.EmployeeIds.Contains(workContext.EmployeeId ?? Guid.Empty));

            if (q.StockId.HasValue) query = query.Where(w => w.StockId == q.StockId);
            if (q.ProductId.HasValue) query = query.Where(w => w.Sku!.ProductId == q.ProductId);
            if (q.ManufacturerId.HasValue) query = query.Where(w => w.Sku!.ManufacturerId == q.ManufacturerId);
            if (!string.IsNullOrEmpty(q.Seria))
            {
                var s = q.Seria.ToLower();
                query = query.Where(w => w.Sku!.SerialNumber.ToLower().Contains(s));
            }

            var result = await query.OrderBy(a => a.Stock!.Name)
                .ToPagedResultAsync<StockSku, StockSkuModel>(q.Paging, mapper, ct);

            await ChangeMeasurementUnitsAsync(result, ct);
            return result;
        }

        private async Task ChangeMeasurementUnitsAsync(PagedResult<StockSkuModel> result, CancellationToken ct)
        {
            var muTypes = result.Items.Where(w => w.MeasurementUnit != null).Select(s => s.MeasurementUnit!.Type).ToList();
            var mus = await db.OrganizationMeasurementUnits
                .Where(f => f.OrganizationId == workContext.TenantId && muTypes.Contains(f.Type))
                .Select(s => s.MeasurementUnit).ToListAsync(ct);
            result.Items.ForEach(item =>
            {
                if (item.MeasurementUnit == null) return;
                var mu = mus.FirstOrDefault(f => f!.Type == item.MeasurementUnit.Type);
                if (mu == null) return;
                var muModel = mapper.Map<MeasurementUnitModel>(mu);
                item.MeasurementUnit = muModel;
                item.Amount /= muModel.Multiplier > 0 ? muModel.Multiplier : 1;
            });
        }
    }
}
