namespace Ucms.Application.Features.CashAccounts.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Extensions;
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
            var query = ctx.OrganizationId.HasValue
                ? db.CashAccounts.IncludeChilds(ctx.OrganizationId.Value)
                : db.CashAccounts.AsQueryable();

            if (q.IsActive.HasValue)
                query = query.Where(a => a.IsActive == q.IsActive.Value);

            if (q.Type.HasValue)
                query = query.Where(a => a.Type == q.Type.Value);

            // Balance endi CashAccount jadvalida to'g'ridan-to'g'ri saqlanadi (denormalizatsiya).
            // apply_cash_balance_delta() SP har bir write da sinxron yangilaydi.
            var items = await query
                .OrderBy(a => a.Name)
                .Select(a => new Item(
                    a.Id, a.Name, a.Type, a.Notes, a.IsActive,
                    a.Balance,
                    a.CreatedAt))
                .ToListAsync(ct);

            return (new Result(items.Sum(i => i.Balance), items), false);
        }
    }
}
