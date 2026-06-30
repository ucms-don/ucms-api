namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateAccountTransfer
{
    public record Command(
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount,
        decimal Commission,
        string TransferredBy,
        DateTimeOffset Date,
        string? Note);

    public record Result(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool FromNotFound, bool ToNotFound, bool Forbidden, string? Error)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            if (!ctx.IsOwner && !ctx.OrganizationId.HasValue)
                return (null, false, false, true, null);

            var fromAccount = await db.CashAccounts
                .Where(a => a.Id == cmd.FromAccountId && !a.IsDeleted)
                .Select(a => new { a.Id, a.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (fromAccount is null) return (null, true, false, false, null);
            if (!ctx.IsOwner && ctx.OrganizationId != fromAccount.OrganizationId)
                return (null, false, false, true, null);

            var toAccount = await db.CashAccounts
                .Where(a => a.Id == cmd.ToAccountId && !a.IsDeleted)
                .Select(a => new { a.Id, a.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (toAccount is null) return (null, false, true, false, null);

            if (cmd.FromAccountId == cmd.ToAccountId)
                return (null, false, false, false, "Manba va maqsad kassa bir xil bo'lishi mumkin emas");

            if (cmd.Amount <= 0)
                return (null, false, false, false, "Summa 0 dan katta bo'lishi kerak");

            if (cmd.Commission < 0)
                return (null, false, false, false, "Komissiya manfiy bo'lishi mumkin emas");

            // Manba hisobda yetarli mablag' borligini tekshirish
            var totalDeducted = cmd.Amount + cmd.Commission;
            var available = await CashTransactionLinker.GetAvailableBalanceAsync(
                db, cmd.FromAccountId, null, null, ct);

            if (available < totalDeducted)
                return (null, false, false, false,
                    $"Kassada mablag' yetarli emas. " +
                    $"Mavjud balans: {available:N2} so'm, " +
                    $"kerakli: {totalDeducted:N2} so'm " +
                    $"(o'tkazma: {cmd.Amount:N2} + komissiya: {cmd.Commission:N2})");

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var transfer = new AccountTransfer
            {
                Id             = Guid.NewGuid(),
                OrganizationId = fromAccount.OrganizationId,
                FromAccountId  = cmd.FromAccountId,
                ToAccountId    = cmd.ToAccountId,
                Amount         = cmd.Amount,
                Commission     = cmd.Commission,
                TransferredBy  = cmd.TransferredBy,
                Date           = cmd.Date,
                Note           = cmd.Note,
                IsDeleted      = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = userId, UpdatedBy = userId,
            };

            await db.AccountTransfers.AddAsync(transfer, ct);

            // Manba hisobdan chiqim (amount + commission)
            await CashTransactionLinker.UpsertAsync(
                db,
                CashTransactionSourceType.AccountTransferOut, transfer.Id,
                fromAccount.OrganizationId, cmd.FromAccountId,
                CashDirection.Out, CashTransactionType.AccountTransfer,
                FinancePartnerType.Other, null,
                totalDeducted, cmd.Date, null,
                cmd.Note ?? $"O'tkazma chiqimi: {cmd.Amount:N0} so'm + {cmd.Commission:N0} komissiya",
                userId, ct);

            // Maqsad hisobga kirim (faqat amount, commission emas)
            await CashTransactionLinker.UpsertAsync(
                db,
                CashTransactionSourceType.AccountTransferIn, transfer.Id,
                fromAccount.OrganizationId, cmd.ToAccountId,
                CashDirection.In, CashTransactionType.AccountTransfer,
                FinancePartnerType.Other, null,
                cmd.Amount, cmd.Date, null,
                cmd.Note ?? $"O'tkazma kirimi: {cmd.Amount:N0} so'm",
                userId, ct);

            await db.SaveChangesAsync(ct);
            return (new Result(transfer.Id), false, false, false, null);
        }
    }
}
