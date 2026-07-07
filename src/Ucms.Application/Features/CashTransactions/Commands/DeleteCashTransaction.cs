namespace Ucms.Application.Features.CashTransactions.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteCashTransaction
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var snapshot = await db.CashTransactions
                .Where(t => t.Id == cmd.Id)
                .Select(t => new { t.OrganizationId, t.CashAccountId, t.Direction, t.Amount, t.IsDeleted })
                .FirstOrDefaultAsync(ct);

            if (snapshot is null || snapshot.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != snapshot.OrganizationId) return (false, true);

            var userId     = ctx.UserId ?? Guid.Empty;
            var reverseDir = snapshot.Direction == CashDirection.In ? CashDirection.Out : CashDirection.In;

            await db.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ClearChangeTracker();
                await using var tx = await db.BeginTransactionAsync(ct);

                var t = await db.CashTransactions.FindAsync([cmd.Id], ct);
                if (t is null || t.IsDeleted) return;

                t.IsDeleted = true;
                t.UpdatedAt = DateTimeOffset.UtcNow;
                t.UpdatedBy = userId;
                db.CashTransactions.Update(t);
                await db.SaveChangesAsync(ct);

                // Balansni teskari qaytarish (allowOverdraft=true — bu undo operatsiyasi)
                await balanceService.ApplyDeltaAsync(
                    snapshot.CashAccountId, snapshot.Amount, reverseDir,
                    allowOverdraft: true, ct: ct);

                await tx.CommitAsync(ct);
            });

            return (false, false);
        }
    }
}
