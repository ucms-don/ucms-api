namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class CancelAccountTransfer
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool AlreadyCancelled)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var transfer = await db.AccountTransfers
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Id == cmd.Id && !t.IsDeleted, ct);

            if (transfer is null) return (true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != transfer.OrganizationId)
                return (false, true, false);
            if (transfer.IsCancelled) return (false, false, true);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var tx = await db.BeginTransactionAsync(ct);
                db.ClearChangeTracker();

                // Ikki tomonlama o'tkazmani teskari qaytarish
                await CashTransactionLinker.RemoveAsync(
                    db, balanceService,
                    CashTransactionSourceType.AccountTransferOut, cmd.Id, ct);

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService,
                    CashTransactionSourceType.AccountTransferIn, cmd.Id, ct);

                var t = await db.AccountTransfers.AsTracking().FirstAsync(x => x.Id == cmd.Id, ct);
                t.IsCancelled = true;
                t.CancelledAt = DateTimeOffset.UtcNow;
                t.CancelledBy = ctx.UserId ?? Guid.Empty;
                t.UpdatedAt   = DateTimeOffset.UtcNow;
                t.UpdatedBy   = ctx.UserId ?? Guid.Empty;

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, false, false);
        }
    }
}
