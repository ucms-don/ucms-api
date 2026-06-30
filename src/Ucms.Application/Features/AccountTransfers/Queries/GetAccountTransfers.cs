namespace Ucms.Application.Features.AccountTransfers.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetAccountTransfers
{
    public record Query(
        Guid? FromAccountId,
        Guid? ToAccountId,
        DateTimeOffset? DateFrom,
        DateTimeOffset? DateTo,
        int Page,
        int Size);

    public record Item(
        Guid   Id,
        Guid   FromAccountId,
        string FromAccountName,
        Guid   ToAccountId,
        string ToAccountName,
        decimal Amount,
        decimal Commission,
        string TransferredBy,
        DateTimeOffset Date,
        string? Note,
        DateTimeOffset CreatedAt);

    public record Result(int Total, decimal TotalAmount, decimal TotalCommission, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!ctx.IsOwner && !ctx.OrganizationId.HasValue)
                return (null, true);

            var query = db.AccountTransfers.AsQueryable();

            if (!ctx.IsOwner && ctx.OrganizationId.HasValue)
                query = query.Where(t => t.OrganizationId == ctx.OrganizationId.Value);

            if (q.FromAccountId.HasValue)
                query = query.Where(t => t.FromAccountId == q.FromAccountId.Value);

            if (q.ToAccountId.HasValue)
                query = query.Where(t => t.ToAccountId == q.ToAccountId.Value);

            if (q.DateFrom.HasValue)
                query = query.Where(t => t.Date >= q.DateFrom.Value);

            if (q.DateTo.HasValue)
                query = query.Where(t => t.Date <= q.DateTo.Value);

            var total           = await query.CountAsync(ct);
            var totalAmount     = await query.SumAsync(t => t.Amount, ct);
            var totalCommission = await query.SumAsync(t => t.Commission, ct);

            var items = await query
                .OrderByDescending(t => t.Date)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(t => new Item(
                    t.Id,
                    t.FromAccountId, t.FromAccount!.Name,
                    t.ToAccountId,   t.ToAccount!.Name,
                    t.Amount, t.Commission, t.TransferredBy,
                    t.Date, t.Note, t.CreatedAt))
                .ToListAsync(ct);

            return (new Result(total, totalAmount, totalCommission, items), false);
        }
    }
}
