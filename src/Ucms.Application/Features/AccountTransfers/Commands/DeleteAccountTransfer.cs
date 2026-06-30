namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteAccountTransfer
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var transfer = await db.AccountTransfers
                .FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);

            if (transfer is null) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != transfer.OrganizationId)
                return (false, true);

            var userId = ctx.UserId ?? Guid.Empty;

            transfer.IsDeleted  = true;
            transfer.UpdatedAt  = DateTimeOffset.UtcNow;
            transfer.UpdatedBy  = userId;

            db.AccountTransfers.Update(transfer);

            // Bog'langan CashTransaction'larni soft-delete qilish
            await CashTransactionLinker.RemoveAsync(
                db, CashTransactionSourceType.AccountTransferOut, cmd.Id, userId, ct);
            await CashTransactionLinker.RemoveAsync(
                db, CashTransactionSourceType.AccountTransferIn, cmd.Id, userId, ct);

            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
