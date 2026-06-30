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
                    ClientTotal    = Math.Round(i.Volume * i.ClientUnitPrice,    2),
                    BrigadeTotal   = Math.Round(i.Volume * i.BrigadeUnitPrice,   2),
                    MaterialTotal  = Math.Round(i.Volume * i.MaterialUnitPrice,  2),
                    VatAmount      = Math.Round(i.Volume * i.ClientUnitPrice * i.VatRate / 100m,                                     2),
                    TotalWithVat   = Math.Round(i.Volume * i.MaterialUnitPrice + i.Volume * i.ClientUnitPrice * (1m + i.VatRate / 100m), 2),
                })
                .ToListAsync(ct);

 