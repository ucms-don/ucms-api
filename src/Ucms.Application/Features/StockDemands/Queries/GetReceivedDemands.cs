namespace Ucms.Application.Features.StockDemands.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Authorization;
using Ucms.Application.Abstractions.Constants;
using Ucms.Application.Features.StockDemands.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class GetReceivedDemands
{
    public record Query(PagedRequest Paging, DateTime? From, DateTime? To, string? Name);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext, IPermissionProvider permissionProvider)
    {
        public async Task<PagedResult<ReceivedDemandModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.StockDemands
                .Include(i => i.Sender)
                .Include(i => i.StockDemandItems).ThenInclude(th => th.Product)
                .Include(i => i.StockDemandItems).ThenInclude(th => th.MeasurementUnit)
                .Where(w => w.DemandStatus != StockDemandStatus.Draft);

            if (!workContext.IsAdmin)
                query = query.Where(w => w.Recipient!.OrganizationId == workContext.TenantId);

            if (!await permissionProvider.HasPermissionAsync(Permissions.Warehouse.AccessSettingMinimumBalanceWarehouse, ct))
                query = query.Where(w => w.Recipient!.EmployeeIds.Contains(workContext.EmployeeId ?? Guid.Empty));

            if (q.From != null && q.To != null)
                query = query.Where(w => w.DemandDate.Date >= q.From.Value.Date && w.DemandDate.Date <= q.To.Value.Date);

            if (!string.IsNullOrEmpty(q.Name))
            {
                var n = q.Name.ToLower();
                query = query.Where(w => w.Name.ToLower().Contains(n));
            }

            return await query.OrderBy(c => c.Name)
                .ToPagedResultAsync<StockDemand, ReceivedDemandModel>(q.Paging, mapper, ct);
        }
    }
}
