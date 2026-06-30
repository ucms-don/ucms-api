namespace Ucms.Application.Features.CashAccounts.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetCashAccounts
{
    public record Query(bool? IsActive, CashAccountType? Type);

    public record Item(
        Guid Id, string Name, CashAccountType Type, string? Notes,
        bool IsActive, decimal Balance, DateTimeOffset CreatedAt);

    public record Result(decimal TotalBalance, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!ctx.OrganizationId.HasValue) 
                return (null, true);

            var query = db.CashAccounts.Where(a => !a.IsDeleted
            && a.OrganizationId == ctx.OrganizationId!.Value);

            if (q.IsActive.HasValue)
                query = query.Where(a => a.IsActive == q.IsActive.Value);

            if (q.Type.HasValue)
                query = query.Where(a => a.Type == q.Type.Value);

            var items = await query
                .OrderBy(a => a.Name)
                .Select(a => new Item(
                    a.Id, a.Name, a.Type, a.Notes, a.IsActive,
                    a.Transactions.Where(t => !t.IsDeleted)
                        .Sum(t => t.Direction == CashDirection.In ? t.Amount : -t.Amount),
                    a.CreatedAt))
                .ToListAsync(ct);

            return (new Result(items.Sum(i => i.Balance), items), false);
        }
    }
}
