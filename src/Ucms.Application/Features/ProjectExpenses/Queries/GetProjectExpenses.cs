namespace Ucms.Application.Features.ProjectExpenses.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetProjectExpenses
{
    public record Query(Guid ProjectId, string? Category, DateTimeOffset? From, DateTimeOffset? To, int Page, int Size);

    public record Item(
        Guid Id, DateTimeOffset Date, string Category, decimal Amount,
        string? Description, string? PaymentMethod, string? Note, DateTimeOffset CreatedAt);

    public record Result(int Total, int Page, int Size, decimal TotalAmount, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var query = db.ProjectExpenses.Where(e => e.ProjectId == q.ProjectId && !e.IsDeleted);

            if (!string.IsNullOrEmpty(q.Category))
                query = query.Where(e => e.Category == q.Category);
            if (q.From.HasValue)
                query = query.Where(e => e.Date >= q.From.Value);
            if (q.To.HasValue)
                query = query.Where(e => e.Date <= q.To.Value);

            var total       = await query.CountAsync(ct);
            var totalAmount = await query.SumAsync(e => e.Amount, ct);

            var items = await query
                .OrderByDescending(e => e.Date)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(e => new Item(
                    e.Id, e.Date, e.Category, e.Amount,
                    e.Description, e.PaymentMethod, e.Note, e.CreatedAt))
                .ToListAsync(ct);

            return (new Result(total, q.Page, q.Size, totalAmount, items), false, false);
        }
    }
}
