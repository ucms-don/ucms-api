namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateAccountTransfer
{
    public record Command(
        Guid Id, Guid FromAccountId, Guid ToAccountId,
        decimal Amount, decimal Commission,
        string TransferredBy, DateTimeOffset Date, string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool FromNotFound, bool ToNotFound, string? Error)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var transfer = await db.AccountTransfers
                .FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);

            if (transfer is null) return (true, false, false, false, null);
            if (!ctx.IsOwner && ctx.OrganizationId != transfer.OrganizationId)
                return (false, true, false, false, null);

            if (!await db.CashAccounts.AnyAsync(a => a.Id == cmd.FromAccountId, ct))
                return (false, false, true, false, null);
            if (!await db.CashAccounts.AnyAsync(a => a.Id == cmd.ToAccountId, ct))
                return (false, false, false, true, null);

            if (cmd.FromAccountId == cmd.ToAccountId)
                return (false, false, false, false, "Manba va maqsad kassa bir xil bo'lishi mumkin emas");
            if (cmd.Amount <= 0)
                return (false, false, false, false, "Summa 0 dan katta bo'lishi kerak");
            if (cmd.Commission < 0)
                return (false, false, false, false, "Komissiya manfiy bo'lishi mumkin emas");

            var totalDeducted = cmd.Amount + cmd.Commission;
            var userId        = ctx.UserId ?? Guid.Empty;
            var orgId         = transfer.OrganizationId;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var t = await db.AccountTransfers.FindAsync([cmd.Id], ct);
                    if (t is null) return;
                    t.FromAccountId = cmd.FromAccountId; t.ToAccountId  = cmd.ToAccountId;
                    t.Amount        = cmd.Amount;         t.Commission   = cmd.Commission;
                    t.TransferredBy = cmd.TransferredBy;  t.Date         = cmd.Date;
                    t.Note          = cmd.Note;            t.UpdatedAt    = DateTimeOffset.UtcNow;
                    t.UpdatedBy     = userId;
                    db.AccountTransfers.Update(t);

                    await CashTransactionLinker.UpsertAsync(
                        db, balanceService,
                        CashTransactionSourceType.AccountTransferOut, cmd.Id,
                        orgId, cmd.FromAccountId,
                        CashDirection.Out, CashTransactionType.AccountTransfer,
                        FinancePartnerType.Other, null,
                        totalDeducted, cmd.Date, null,
                        cmd.Note ?? $"O'tkazma chiqimi: {cmd.Amount:N0} so'm + {cmd.Commission:N0} komissiya",
                        userId, ct);

                    await CashTransactionLinker.UpsertAsync(
                        db, balanceService,
                        CashTransactionSourceType.AccountTransferIn, cmd.Id,
                        orgId, cmd.ToAccountId,
                        CashDirection.In, CashTransactionType.AccountTransfer,
                        FinancePartnerType.Other, null,
                        cmd.Amount, cmd.Date, null,
                        cmd.Note ?? $"O'tkazma kirimi: {cmd.Amount:N0} so'm",
                        userId, ct);

                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException ex)
            {
                return (false, false, false, false, ex.Message);
            }

            return (false, false, false, false, null);
        }
    }
}
