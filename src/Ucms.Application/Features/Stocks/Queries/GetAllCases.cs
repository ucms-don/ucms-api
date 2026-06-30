namespace Ucms.Application.Features.Stocks.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetAllCases
{
    public record Query;

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<List<StockModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var stocks = await db.Stocks
                .Where(w => w.OrganizationId == workContext.TenantId && w.StockType == StockType.Case)
                .OrderBy(a => a.Name).ToListAsync(ct);
            return mapper.Map<List<StockModel>>(stocks);
        }
    }
}
