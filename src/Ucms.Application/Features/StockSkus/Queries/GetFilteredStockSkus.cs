namespace Ucms.Application.Features.StockSkus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Features.StockSkus.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetFilteredStockSkus
{
    public record Query(PagedRequest Paging);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<PagedResult<StockSkuModel>> HandleAsync(Query q, CancellationToken ct)
        {
            return await db.StockSkus
            .Include(i => i.Stock).Include(i => i.Sku)
            .OrderBy(a => a.Stock!.Name)
            .ToPagedResultAsync<StockSku, StockSkuModel>(q.Paging, mapper, ct);
        }
    }
}
