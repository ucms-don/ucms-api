namespace Ucms.Application.Features.OrganizationMeasurementUnits.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.OrganizationMeasurementUnits.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetFilteredOrganizationMeasurementUnits
{
    public record Query(PagedRequest Paging);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<PagedResult<OrganizationMeasurementUnitModel>> HandleAsync(Query q, CancellationToken ct)
        {
            return await db.OrganizationMeasurementUnits
                .Include(i => i.MeasurementUnit)
                .Where(w => w.OrganizationId == workContext.TenantId)
                .ToPagedResultAsync<OrganizationMeasurementUnit, OrganizationMeasurementUnitModel>(q.Paging, mapper, ct);
        }
    }
}
