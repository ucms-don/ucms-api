namespace Ucms.Application.Features.Stocks.Queries;

using AutoMapper;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Authorization;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetFilteredStocks
{
    public record Query(PagedRequest Paging);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext, IPermissionProvider permissionProvider)
    {
        public async Task<PagedResult<StockModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.Stocks
                .Where(w => w.OrganizationId == workContext.TenantId);
            
            return await query
                .OrderBy(c => c.Name)
                .ToPagedResultAsync<Stock, StockModel>(q.Paging, mapper, ct);
        }
    }
}
