namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateAccountTransfer
{
    public record Command(
        Guid Id,
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount,
        decimal Commission,
        string TransferredBy,
        DateTimeOffset Date,
        string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, bool FromNotFound, bool ToNotFound, string? Error)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var transfer = await db.AccountTransfers
                .FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);

            if (transfer is null) return (true, false, false, false, null);
            if (!ctx.IsOwner && ctx.OrganizationId != transfer.OrganizationId)
                return (false, true, false, false, null);

            var fromExists = await db.CashAccounts
                .AnyAsync(a => a.Id == cmd.FromAccountId && !a.IsDeleted, ct);
            if (!fromExists) return (false, false, true, false, null);

            var toExists = await db.CashAccounts
                .AnyAsync(a => a.Id == cmd.ToAccountId && !a.IsDeleted, ct);
            if (!toExists) return (false, false, false, true, null);

            if (cmd.FromAccountId == cmd.ToAccountId)
                return (false, false, false, false, "Manba va maqsad kassa bir xil bo'lishi mumkin emas");

            if (cmd.Amount <= 0)
                return (false, false, false, false, "Summa 0 dan katta bo'lishi kerak");

            if (cmd.Commission < 0)
                return (false, false, false, false, "Komissiya manfiy bo'lishi mumkin emas");

            // Balans tekshiruvi — eski Out tranzaksiyani hisobdan chiqarib
            var totalDeducted = cmd.Amount + cmd.Commission;
            var available = await CashTransactionLinker.GetAvailableBalanceAsync(
                db, cmd.FromAccountId,
                CashTransactionSourceType.AccountTransferOut, cmd.Id,
                ct);

            if (available < totalDeducted)
                return (false, false, false, false,
                    $"Kassada mablag' yetarli emas. " +
                    $"Mavjud balans: {available:N2} so'm, " +
                    $"kerakli: {totalDeducted:N2} so'm " +
                    $"(o'tkazma: {cmd.Amount:N2} + komissiya: {cmd.Commission:N2})");

            var userId = ctx.UserId ?? Guid.Empty;

            transfer.FromAccountId = cmd.FromAccountId;
            transfer.ToAccountId   = cmd.ToAccountId;
            transfer.Amount        = cmd.Amount;
            transfer.Commission    = cmd.Commission;
            transfer.TransferredBy = cmd.TransferredBy;
            transfer.Date          = cmd.Date;
            transfer.Note          = cmd.Note;
            transfer.UpdatedAt     = DateTimeOffset.UtcNow;
            transfer.UpdatedBy     = userId;

            db.AccountTransfers.Update(transfer);

            // Manba hisobdagi chiqim tranzaksiyasini yangilash
            await CashTransactionLinker.UpsertAsync(
                db,
                CashTransactionSourceType.AccountTransferOut, cmd.Id,
                transfer.OrganizationId, cmd.FromAccountId,
                CashDirection.Out, CashTransactionType.AccountTransfer,
                FinancePartnerType.Other, null,
                totalDeducted, cmd.Date, null,
                cmd.Note ?? $"O'tkazma chiqimi: {cmd.Amount:N0} so'm + {cmd.Commission:N0} komissiya",
                userId, ct);

            // Maqsad hisobdagi kirim tranzaksiyasini yangilash
            await CashTransactionLinker.UpsertAsync(
                db,
                CashTransactionSourceType.AccountTransferIn, cmd.Id,
                transfer.OrganizationId, cmd.ToAccountId,
                CashDirection.In, CashTransactionType.AccountTransfer,
                FinancePartnerType.Other, null,
                cmd.Amount, cmd.Date, null,
                cmd.Note ?? $"O'tkazma kirimi: {cmd.Amount:N0} so'm",
                userId, ct);

            await db.SaveChangesAsync(ct);
            return (false, false, false, false, null);
        }
    }
}
