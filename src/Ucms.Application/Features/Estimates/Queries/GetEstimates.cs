namespace Ucms.Application.Features.Estimates.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetEstimates
{
    public record Query(Guid ProjectId);

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

            var estimates = await db.Estimates
                .Where(e => e.ProjectId == q.ProjectId)
                .OrderBy(e => e.Order)
                .Select(e => (object)new
                {
                    e.Id, e.Name, e.Description, e.Order,
                    SectionCount  = e.Sections.Count,
                    ClientTotal   = Math.Round(e.Sections.SelectMany(s => s.EstimateItems).Sum(i => i.Volume * i.ClientUnitPrice),   2),
                    BrigadeTotal  = Math.Round(e.Sections.SelectMany(s => s.EstimateItems).Sum(i => i.Volume * i.BrigadeUnitPrice),  2),
                    MaterialTotal = Math.Round(e.Sections.SelectMany(s => s.EstimateItems).Sum(i => i.Volume * i.MaterialUnitPrice), 2),
                    VatAmount     = Math.Round(e.Sections.SelectMany(s => s.EstimateItems).Sum(i => i.Volume * i.ClientUnitPrice * i.VatRate / 100m), 2),
                    TotalWithVat  = Math.Round(e.Sections.SelectMany(s => s.EstimateItems).Sum(i => i.Volume * i.MaterialUnitPrice + i.Volume * i.ClientUnitPrice * (1m + i.VatRate / 100m)), 2),
                    CreatedAt = e.CreatedAt,
                })
                .ToListAsync(ct);

            return (estimates, false);
        }
    }
}
