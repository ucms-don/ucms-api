namespace Ucms.Application.Features.Estimates.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetItems
{
    public record Query(Guid ProjectId, Guid EstimateId, Guid SectionId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(List<object>? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null || (!ctx.IsOwner && ctx.OrganizationId != orgId))
                return (null, orgId is not null);

            var items = await db.EstimateItems
                .Where(i => i.SectionId == q.SectionId && i.Section!.EstimateId == q.EstimateId)
                .OrderBy(i => i.Order)
                .Select(i => (object)new
                {
                    i.Id, i.Description, i.Order,
                    i.SurfaceType,
                    WorkTypeId          = i.WorkTypeId,
                    WorkTypeName        = i.WorkType!.Name,
                    MeasurementUnitId   = i.MeasurementUnitId,
                    MeasurementUnitCode = i.MeasurementUnit!.Code,
                    i.Volume,
                    i.ClientUnitPrice,
                    i.BrigadeUnitPrice,
                    i.MaterialUnitPrice,
                    i.VatRate,
                    ClientTotal    = i.Volume * i.ClientUnitPrice,
                    BrigadeTotal   = i.Volume * i.BrigadeUnitPrice,
                    MaterialTotal  = i.Volume * i.MaterialUnitPrice,
                    VatAmount      = i.Volume * i.ClientUnitPrice * i.VatRate / 100,
                    TotalWithVat   = i.Volume * i.ClientUnitPrice * (1 + i.VatRate / 100),
                })
                .ToListAsync(ct);

            return (items, false);
        }
    }
}
