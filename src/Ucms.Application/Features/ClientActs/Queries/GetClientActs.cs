namespace Ucms.Application.Features.ClientActs.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetClientActs
{
    public record Query(Guid ProjectId, ActStatus? Status);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(List<object>? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var query = db.ClientActs.Where(a => a.ProjectId == q.ProjectId);
            if (q.Status.HasValue) query = query.Where(a => a.Status == q.Status.Value);

            var list = await query
                .OrderByDescending(a => a.ActDate)
                .Select(a => (object)new
                {
                    a.Id, a.ActNumber, a.ActDate, a.TotalAmount, a.Status, a.Note,
                    PaidAmount = a.Payments.Sum(p => p.Amount),
                    ItemCount  = a.Items.Count,
                })
                .ToListAsync(ct);

            return (list, false, false);
        }
    }
}
