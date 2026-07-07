namespace Ucms.Application.Features.CashTransactions.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class CancelCashTransaction
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool AlreadyCancelled)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var tx = await db.CashTransactions
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Id == cmd.Id && !t.IsDeleted, ct);

            if (tx is null) return (true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != tx.OrganizationId)
                return (false, true, false);
            if (tx.IsCancelled) return (false, false, true);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var dbTx = await db.BeginTransactionAsync(ct);
                db.ClearChangeTracker();

                // Balansni teskari qaytarish
                var reverseDir = tx.Direction == CashDirection.In ? CashDirection.Out : CashDirection.In;
                await balanceService.ApplyDeltaAsync(tx.CashAccountId, tx.Amount, reverseDir, allowOverdraft: true, ct: ct);

                var t = await db.CashTransactions.AsTracking().FirstAsync(x => x.Id == cmd.Id, ct);
                t.IsCancelled = true;
                t.IsDeleted   = true;   // log view dan ham yashirish
                t.CancelledAt = DateTimeOffset.UtcNow;
                t.CancelledBy = ctx.UserId ?? Guid.Empty;
                t.UpdatedAt   = DateTimeOffset.UtcNow;
                t.UpdatedBy   = ctx.UserId ?? Guid.Empty;

                await db.SaveChangesAsync(ct);
                await dbTx.CommitAsync(ct);
            });

            return (false, false, false);
        }
    }
}
