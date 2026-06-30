namespace Ucms.Application.Features.Skus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class GetFilteredSkus
{
    public record Query(PagedRequest Paging, string? Search, string? SerialNumber);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<PagedResult<SkuModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.OrganizationSkus
                .Include(i => i.Sku!.Product)
                .Include(i => i.Sku!.Manufacturer)
                .Include(i => i.Sku!.MeasurementUnit)
                .Include(i => i.Sku!.Supplier)
                .Where(w => w.OrganizationId == workContext.TenantId)
                .Select(s => s.Sku!);

            if (!string.IsNullOrEmpty(q.Search))
            {
                var s = q.Search.ToLowerInvariant().Trim();
                query = query.Where(x =>
                    x.Product!.Name.ToLower().Contains(s) || x.Product!.NameRu.ToLower().Contains(s) ||
                    x.Product!.NameKa!.ToLower().Contains(s) || x.Product!.NameEn!.ToLower().Contains(s) ||
                    x.SerialNumber.ToLower().Contains(s));
            }
            if (!string.IsNullOrEmpty(q.SerialNumber))
            {
                var sn = q.SerialNumber.ToLowerInvariant().Trim();
                query = query.Where(w => w.SerialNumber.ToLower().Contains(sn));
            }
            var result = await query.ToPagedResultAsync<Sku, SkuModel>(q.Paging, mapper, ct);

            // Har bir SKU uchun bog'langan to'lov (SourceType=SkuPurchase) qaysi kassa/bankdan
            // qilinganini bitta so'rovda olib, edit oynasida preload bo'lishi uchun to'ldiramiz.
            var ids = result.Items.Select(i => i.Id).ToList();
            if (ids.Count > 0)
            {
                var links = await db.CashTransactions
                    .Where(t => t.SourceType == CashTransactionSourceType.SkuPurchase
                        && !t.IsDeleted && t.SourceId != null && ids.Contains(t.SourceId.Value))
                    .Select(t => new { SkuId = t.SourceId!.Value, t.CashAccountId })
                    .ToListAsync(ct);
                var map = links.ToDictionary(x => x.SkuId, x => x.CashAccountId);
                result.Items.ForEach(item =>
                {
                    if (map.TryGetValue(item.Id, out var accId)) item.CashAccountId = accId;
                });
            }

            return result;
        }
    }
}
