namespace Ucms.Application.Features.Stocks.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Application.Persistence;

public static class FindStocks
{
    public record Query(string Search);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<List<StockModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var s = q.Search.ToLower();
            var stocks = db.Stocks.Where(a =>
                a.Name.ToLower().Contains(s) || a.NameEn!.ToLower().Contains(s) ||
                a.NameRu.ToLower().Contains(s) || a.NameKa!.ToLower().Contains(s) ||
                a.Code.Contains(s));
            if (!workContext.IsAdmin) stocks = stocks.Where(a => a.OrganizationId == workContext.TenantId);
            return await stocks.OrderBy(a => a.Name)
                .ProjectTo<StockModel>(mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }
    }
}
