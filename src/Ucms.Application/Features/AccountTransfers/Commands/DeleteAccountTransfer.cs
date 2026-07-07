namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteAccountTransfer
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var transfer = await db.AccountTransfers
                .FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);

            if (transfer is null) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != transfer.OrganizationId)
                return (false, true);

            var userId = ctx.UserId ?? Guid.Empty;

            await db.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ClearChangeTracker();
                await using var tx = await db.BeginTransactionAsync(ct);

                var t = await db.AccountTransfers.FindAsync([cmd.Id], ct);
                if (t is null) return;
                t.IsDeleted = true;
                t.UpdatedAt = DateTimeOffset.UtcNow;
                t.UpdatedBy = userId;
                db.AccountTransfers.Update(t);

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService, CashTransactionSourceType.AccountTransferOut, cmd.Id, userId, ct);
                await CashTransactionLinker.RemoveAsync(
                    db, balanceService, CashTransactionSourceType.AccountTransferIn, cmd.Id, userId, ct);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, false);
        }
    }
}
