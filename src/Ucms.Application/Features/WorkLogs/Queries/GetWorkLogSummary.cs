namespace Ucms.Application.Features.WorkLogs.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetWorkLogSummary
{
    public record Query(Guid ProjectId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(List<object>? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var summary = await db.WorkLogs
                .Where(w => w.ProjectId == q.ProjectId)
                .GroupBy(w => w.BrigadeId)
                .Select(g => (object)new
                {
                    BrigadeId   = g.Key,
                    BrigadeName = g.First().Brigade!.Name,
                    TotalAmount = g.Sum(w => w.TotalAmount),
                    PaidAmount  = g.Where(w => w.Status == WorkLogStatus.Paid).Sum(w => w.TotalAmount),
                    Confirmed   = g.Count(w => w.Status == WorkLogStatus.Confirmed),
                    Draft       = g.Count(w => w.Status == WorkLogStatus.Draft),
                })
                .ToListAsync(ct);

            return (summary, false, false);
        }
    }
}
