namespace Ucms.Application.Features.CashAccounts.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetCashAccountById
{
    public record Query(Guid Id);

    public record RecentTransaction(
        Guid Id, CashDirection Direction, CashTransactionType TransactionType,
        decimal Amount, DateTimeOffset Date, string? Note);

    public record CashAccountDetailDto(
        Guid Id, string Name, CashAccountType Type, string? Notes,
        bool IsActive, decimal Balance, Guid OrganizationId,
        DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
        List<RecentTransaction> RecentTransactions);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(CashAccountDetailDto? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var account = await db.CashAccounts
                .Where(a => a.Id == q.Id && !a.IsDeleted)
                .Select(a => new CashAccountDetailDto(
                    a.Id, a.Name, a.Type, a.Notes, a.IsActive,
                    a.Transactions.Where(t => !t.IsDeleted)
                        .Sum(t => t.Direction == CashDirection.In ? t.Amount : -t.Amount),
                    a.OrganizationId, a.CreatedAt, a.UpdatedAt,
                    a.Transactions.Where(t => !t.IsDeleted)
                        .OrderByDescending(t => t.Date)
                        .Take(20)
                        .Select(t => new RecentTransaction(
                            t.Id, t.Direction, t.TransactionType, t.Amount, t.Date, t.Note))
                        .ToList()))
                .FirstOrDefaultAsync(ct);

            if (account is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != account.OrganizationId) return (null, true);
            return (account, false);
        }
    }
}
