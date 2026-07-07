namespace Ucms.Application.Features.CashTransactions.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateCashTransaction
{
    public record Command(
        Guid Id, Guid CashAccountId, CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool CashAccountNotFound, bool ProjectNotFound, bool InsufficientBalance)> HandleAsync(Command cmd, CancellationToken ct)
        {
            // snapshot — retry loop tashqarisida
            var snapshot = await db.CashTransactions
                .Where(t => t.Id == cmd.Id)
                .Select(t => new { t.OrganizationId, t.CashAccountId, t.Direction, t.Amount, t.IsDeleted })
                .FirstOrDefaultAsync(ct);

            if (snapshot is null || snapshot.IsDeleted) return (true, false, false, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != snapshot.OrganizationId) return (false, true, false, false, false);

            if (cmd.CashAccountId != snapshot.CashAccountId)
            {
                var accountExists = await db.CashAccounts
                    .AnyAsync(a => a.Id == cmd.CashAccountId && a.OrganizationId == snapshot.OrganizationId, ct);
                if (!accountExists) return (false, false, true, false, false);
            }

            if (cmd.ProjectId.HasValue)
            {
                var projectExists = await db.Projects
                    .AnyAsync(p => p.Id == cmd.ProjectId.Value && p.OrganizationId == snapshot.OrganizationId, ct);
                if (!projectExists) return (false, false, false, true, false);
            }

            var userId     = ctx.UserId ?? Guid.Empty;
            var reverseDir = snapshot.Direction == CashDirection.In ? CashDirection.Out : CashDirection.In;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var t = await db.CashTransactions.FindAsync([cmd.Id], ct);
                    if (t is null || t.IsDeleted) return;

                    // 1. Eski balansni qaytarish
                    await balanceService.ApplyDeltaAsync(
                        snapshot.CashAccountId, snapshot.Amount, reverseDir,
                        allowOverdraft: true, ct: ct);

                    // 2. Transaksiyani yangilash
                    t.CashAccountId   = cmd.CashAccountId;
                    t.Direction       = cmd.Direction;
                    t.TransactionType = cmd.TransactionType;
                    t.PartnerType     = cmd.PartnerType;
                    t.PartnerId       = cmd.PartnerId;
                    t.PartnerName     = cmd.PartnerName;
                    t.Amount          = cmd.Amount;
                    t.Date            = cmd.Date;
                    t.ProjectId       = cmd.ProjectId;
                    t.Note            = cmd.Note;
                    t.UpdatedAt       = DateTimeOffset.UtcNow;
                    t.UpdatedBy       = userId;
                    db.CashTransactions.Update(t);
                    await db.SaveChangesAsync(ct);

                    // 3. Yangi balansni qo'shish
                    await balanceService.ApplyDeltaAsync(
                        cmd.CashAccountId, cmd.Amount, cmd.Direction,
                        allowOverdraft: false, ct: ct);

                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException)
            {
                return (false, false, false, false, true);
            }

            return (false, false, false, false, false);
        }
    }
}
