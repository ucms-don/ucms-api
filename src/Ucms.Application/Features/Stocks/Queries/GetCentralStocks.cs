namespace Ucms.Application.Features.Stocks.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetCentralStocks
{
    public record Query(Guid OrganizationId);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<List<StockModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var stocks = await db.Stocks
                .Where(a => a.StockCategory == StockCategory.Central && a.OrganizationId == q.OrganizationId)
                .OrderBy(a => a.Name).ToListAsync(ct);
            return mapper.Map<List<StockModel>>(stocks);
        }
    }
}
