namespace Ucms.Application.Features.Estimates.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetSections
{
    public record Query(Guid ProjectId, Guid EstimateId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(List<object>? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null || (!ctx.IsOwner && ctx.OrganizationId != orgId))
                return (null, orgId is not null);

            var sections = await db.EstimateSections
                .Where(s => s.EstimateId == q.EstimateId)
                .OrderBy(s => s.Order)
                .Select(s => (object)new
                {
                    s.Id, s.Name, s.Order, s.ParentId,
                    ChildCount    = db.EstimateSections.Count(c => c.ParentId == s.Id),
                    ItemCount     = s.EstimateItems.Count(),
                    ClientTotal   = Math.Round(s.EstimateItems.Sum(i => i.Volume * i.ClientUnitPrice),   2),
                    BrigadeTotal  = Math.Round(s.EstimateItems.Sum(i => i.Volume * i.BrigadeUnitPrice),  2),
                    MaterialTotal = Math.Round(s.EstimateItems.Sum(i => i.Volume * i.MaterialUnitPrice), 2),
                    VatAmount     = Math.Round(s.EstimateItems.Sum(i => (i.Volume * i.ClientUnitPrice + i.Volume * i.MaterialUnitPrice) * i.VatRate / 100m), 2),
                    TotalWithVat  = Math.Round(s.EstimateItems.Sum(i => (i.Volume * i.ClientUnitPrice + i.Volume * i.MaterialUnitPrice) * (1m + i.VatRate / 100m)), 2),
                })
                .ToListAsync(ct);

            return (sections, false);
        }
    }
}
