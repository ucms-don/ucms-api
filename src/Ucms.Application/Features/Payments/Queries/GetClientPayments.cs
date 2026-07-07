namespace Ucms.Application.Features.Payments.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetClientPayments
{
    public record Query(Guid ProjectId, DateTimeOffset? From, DateTimeOffset? To);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(object? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var query = db.ClientPayments.Where(p => p.ProjectId == q.ProjectId);
            if (q.From.HasValue) query = query.Where(p => p.Date >= q.From.Value);
            if (q.To.HasValue)   query = query.Where(p => p.Date <= q.To.Value);

            var list = await query
                .OrderByDescending(p => p.Date)
                .Select(p => new
                {
                    p.Id, p.Date, p.Amount, p.PaymentMethod, p.Note,
                    ActNumber = p.Act != null ? p.Act.ActNumber : null,
                })
                .ToListAsync(ct);

            return (new { total = list.Sum(p => p.Amount), items = list }, false, false);
        }
    }
}
