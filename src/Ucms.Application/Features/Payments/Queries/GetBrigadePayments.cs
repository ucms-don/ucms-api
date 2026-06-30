namespace Ucms.Application.Features.Payments.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetBrigadePayments
{
    public record Query(Guid ProjectId, Guid? BrigadeId, DateTimeOffset? From, DateTimeOffset? To);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(object? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var query = db.BrigadePayments.Where(p => p.ProjectId == q.ProjectId);
            if (q.BrigadeId.HasValue) query = query.Where(p => p.BrigadeId == q.BrigadeId.Value);
            if (q.From.HasValue)      query = query.Where(p => p.Date >= q.From.Value);
            if (q.To.HasValue)        query = query.Where(p => p.Date <= q.To.Value);

            var list = await query
                .OrderByDescending(p => p.Date)
                .Select(p => new
                {
                    p.Id, p.Date, p.Amount, p.PaymentMethod, p.Note,
                    BrigadeName  = p.Brigade!.Name,
                    WorkLogCount = p.WorkLogs.Count,
                })
                .ToListAsync(ct);

            return (new { total = list.Sum(p => p.Amount), items = list }, false, false);
        }
    }
}
