namespace Ucms.Application.Features.Incomes.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Authorization;
using Ucms.Application.Abstractions.Constants;
using Ucms.Application.Features.Incomes.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetFilteredIncomes
{
    public record Query(PagedRequest Paging, Guid? StockId, string? Search, DateTime? From, DateTime? To);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext, IPermissionProvider permissionProvider)
    {
        public async Task<PagedResult<IncomeModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.Incomes
                .Include(i => i.Stock).Include(i => i.IncomeOutcome!.OutcomeStock)
                .Include(i => i.IncomeItems).ThenInclude(th => th.MeasurementUnit)
                .Include(i => i.IncomeItems).ThenInclude(th => th.Sku!.MeasurementUnit)
                .Where(w => w.Stock!.OrganizationId == workContext.TenantId);

            if (!await permissionProvider.HasPermissionAsync(Permissions.Warehouse.AccessSettingMinimumBalanceWarehouse, ct))
                query = query.Where(w => w.Stock!.EmployeeIds.Contains(workContext.EmployeeId ?? Guid.Empty));

            if (q.StockId.HasValue) query = query.Where(w => w.StockId == q.StockId);
            if (!string.IsNullOrEmpty(q.Search))
            {
                var s = q.Search.ToLowerInvariant().Trim();
                query = query.Where(w => w.Name.ToLower().Contains(s));
            }
            if (q.From != null && q.To != null)
            {
                var from = new DateTime(q.From.Value.Ticks, DateTimeKind.Local);
                var to   = new DateTime(q.To.Value.Ticks, DateTimeKind.Local);
                query = query.Where(w => w.IncomeDate >= from && w.IncomeDate <= to);
            }
            return await query.OrderByDescending(a => a.IncomeDate)
                .ToPagedResultAsync<Income, IncomeModel>(q.Paging, mapper, ct);
        }
    }
}
