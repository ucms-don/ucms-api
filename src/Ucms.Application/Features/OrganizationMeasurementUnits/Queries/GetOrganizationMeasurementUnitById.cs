namespace Ucms.Application.Features.OrganizationMeasurementUnits.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.OrganizationMeasurementUnits.DTOs;
using Ucms.Application.Persistence;

public static class GetOrganizationMeasurementUnitById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<OrganizationMeasurementUnitModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var entity = await db.OrganizationMeasurementUnits.Include(i => i.MeasurementUnit)
                .FirstOrDefaultAsync(f => f.Id == q.Id && f.OrganizationId == workContext.TenantId, ct);
            return entity is null ? null : mapper.Map<OrganizationMeasurementUnitModel>(entity);
        }
    }
}
