namespace Ucms.Application.Features.Stocks.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Application.Persistence;

public static class FindStockByCode
{
    public record Query(string Code);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<StockModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var stock = workContext.IsAdmin
                ? await db.Stocks.FirstOrDefaultAsync(f => f.Code == q.Code, ct)
                : await db.Stocks.FirstOrDefaultAsync(f => f.Code == q.Code && f.OrganizationId == workContext.TenantId, ct);
            return stock is null ? null : mapper.Map<StockModel>(stock);
        }
    }
}
