namespace Ucms.Application.Features.Stocks.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Authorization;
using Ucms.Application.Abstractions.Constants;
using Ucms.Application.Abstractions.Organization;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetStocks
{
    public record Query(Guid OrganizationId, bool? Unattached, StockType? StockType, StockCategory? StockCategory, string? Search, bool? IncludeChild);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext,
        IPermissionProvider permissionProvider, IOrganizationClient organizationClient)
    {
        public async Task<List<StockModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var stocks = db.Stocks.AsQueryable();

            if (workContext.TenantId == q.OrganizationId)
            {
                if (!await permissionProvider.HasPermissionAsync(Permissions.Warehouse.AccessAddWarehouse, ct))
                    stocks = stocks.Where(w => w.EmployeeIds.Contains(workContext.EmployeeId ?? Guid.Empty));
            }

            if (!string.IsNullOrEmpty(q.Search))
            {
                var s = q.Search.ToLower();
                stocks = stocks.Where(a =>
                    a.Name.ToLower().Contains(s) || a.NameRu.ToLower().Contains(s) ||
                    a.NameEn!.ToLower().Contains(s) || a.NameKa!.ToLower().Contains(s) ||
                    a.Code.Contains(s));
            }

            if (q.IncludeChild == true)
            {
                var orgIds = await organizationClient.GetOrganizationIds(includeChilds: true);
                stocks = stocks.Where(w => orgIds.Contains(w.OrganizationId));
            }
            else stocks = stocks.Where(a => a.OrganizationId == q.OrganizationId);

            if (q.StockType.HasValue) stocks = stocks.Where(a => a.StockType == q.StockType);
            if (q.StockCategory.HasValue) stocks = stocks.Where(a => a.StockCategory == q.StockCategory);

            if (q.Unattached == true)
            {
                var stockIds = await organizationClient.GetStockIds(q.OrganizationId);
                stocks = stocks.Where(a => !stockIds.Contains(a.Id));
            }

            return mapper.Map<List<StockModel>>(await stocks.OrderBy(a => a.Name).ToListAsync(ct));
        }
    }
}
