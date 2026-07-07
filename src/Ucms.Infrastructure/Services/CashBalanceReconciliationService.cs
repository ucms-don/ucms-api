namespace Ucms.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ucms.Application.Features.CashAccounts;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public sealed class CashBalanceReconciliationService(
    IUcmsDbContext db,
    ILogger<CashBalanceReconciliationService> logger) : ICashBalanceReconciliationService
{
    public async Task<ReconciliationResult> RunAsync(CancellationToken ct = default)
    {
        var rows = await db.CashAccounts
            .Select(a => new
            {
                a.Id,
                a.Name,
                Stored = a.Balance,
                Real   = a.Transactions
                    .Sum(t => t.Direction == CashDirection.In ? t.Amount : -t.Amount),
            })
            .ToListAsync(ct);

        var mismatches = rows
            .Where(x => x.Stored != x.Real)
            .Select(x => new ReconciliationMismatch(x.Id, x.Name, x.Stored, x.Real, x.Real - x.Stored))
            .ToList();

        if (mismatches.Count == 0)
        {
            logger.LogInformation("Balance reconciliation: all {Total} accounts OK.", rows.Count);
            return new ReconciliationResult(rows.Count, 0, mismatches);
        }

        foreach (var m in mismatches)
            logger.LogWarning(
                "Balance mismatch: account {Id} ({Name}) stored={Stored} real={Real} diff={Diff}",
                m.AccountId, m.AccountName, m.StoredBalance, m.RealBalance, m.Diff);

        var ids      = mismatches.Select(m => m.AccountId).ToList();
        var accounts = await db.CashAccounts
            .AsTracking()
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(ct);

        foreach (var acc in accounts)
        {
            var m = mismatches.First(x => x.AccountId == acc.Id);
            acc.Balance   = m.RealBalance;
            acc.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        logger.LogWarning(
            "Balance reconciliation done: {Fixed}/{Total} accounts corrected.",
            mismatches.Count, rows.Count);

        return new ReconciliationResult(rows.Count, mismatches.Count, mismatches);
    }
}
