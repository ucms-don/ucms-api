namespace Ucms.Application.Features.Manufacturers.Queries;

using AutoMapper;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetStockSkuManufacturers
{
    public record Query(string? Search, Guid? OrganizationId, Guid? StockId, Guid? ProductId, int Page = 1, int Size = 20);

    public sealed class Handler(
        IUcmsDbContext db, 
        IMapper mapper,
        ICurrentContext currentContext)
    {
        public async Task<PagedResult<ManufacturerModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = q.OrganizationId ?? currentContext.OrganizationId;

            var manufacturerIds = db.StockSkus
                .Where(ss =>
                    (ss.Stock!.OrganizationId == orgId) &&
                    (!q.StockId.HasValue || ss.StockId == q.StockId) &&
                    (!q.ProductId.HasValue || ss.Sku!.ProductId == q.ProductId))
                .Select(ss => ss.Sku!.ManufacturerId)
                .Where(id => id != null)
                .Distinct();

            var query = db.Manufacturers
                .Where(m => manufacturerIds.Contains(m.Id))
                .OrderBy(x => x.Name);
            
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.ToLower().Trim();
                query = (IOrderedQueryable<Manufacturer>)query.Where(x =>
                    x.Name.ToLower().Contains(s) || x.NameRu.ToLower().Contains(s) ||
                    (x.NameKa != null && x.NameKa.ToLower().Contains(s)) ||
                    (x.NameEn != null && x.NameEn.ToLower().Contains(s)));
            }
            
            var paged = new PagedRequest { Page = q.Page, PageSize = q.Size };
            return await query.ToPagedResultAsync<Manufacturer, ManufacturerModel>(paged, mapper, ct);
        }
    }
}
