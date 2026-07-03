namespace Ucms.Application.Features.Payments.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetAllBrigadePayments
{
    public record Query(Guid? BrigadeId, Guid? ProjectId, DateTimeOffset? From, DateTimeOffset? To, int Page, int Size);

    public record Item(
        Guid Id, Guid ProjectId, string ProjectName,
        Guid BrigadeId, string BrigadeName,
        DateTimeOffset Date, decimal Amount,
        string PaymentMethod, string? Note,
        int WorkLogCount, DateTimeOffset CreatedAt);

    public record Result(int Total, int Page, int Size, decimal TotalAmount, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!ctx.IsOwner && !ctx.OrganizationId.HasValue) return (null, true);

            var query = db.BrigadePayments
                .Where(p => !p.Project!.IsDeleted);

            if (!ctx.IsOwner && ctx.OrganizationId.HasValue)
                query = query.Where(p => p.Project!.OrganizationId == ctx.OrganizationId.Value);

            if (q.BrigadeId.HasValue)  query = query.Where(p => p.BrigadeId  == q.BrigadeId.Value);
            if (q.ProjectId.HasValue)  query = query.Where(p => p.ProjectId  == q.ProjectId.Value);
            if (q.From.HasValue)       query = query.Where(p => p.CreatedAt >= q.From.Value);
            if (q.To.HasValue)         query = query.Where(p => p.CreatedAt <= q.To.Value);

            var total       = await query.CountAsync(ct);
            var totalAmount = total > 0 ? await query.SumAsync(p => p.Amount, ct) : 0;

            var items = await query
                .OrderByDescending(p => p.Date)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(p => new Item(
                    p.Id,
                    p.ProjectId,
                    p.Project!.Name,
                    p.BrigadeId,
                    p.Brigade!.Name,
                    p.Date,
                    p.Amount,
                    p.PaymentMethod.ToString(),
                    p.Note,
                    p.WorkLogs.Count,
                    p.CreatedAt))
                .ToListAsync(ct);

            return (new Result(total, q.Page, q.Size, totalAmount, items), false);
        }
    }
}
