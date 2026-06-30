namespace Ucms.Application.Features.CashTransactions.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateCashTransaction
{
    public record Command(
        Guid Id, Guid CashAccountId, CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, bool CashAccountNotFound, bool ProjectNotFound)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var transaction = await db.CashTransactions.FindAsync([cmd.Id], ct);
            if (transaction is null || transaction.IsDeleted) return (true, false, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != transaction.OrganizationId) return (false, true, false, false);

            if (cmd.CashAccountId != transaction.CashAccountId)
            {
                var accountExists = await db.CashAccounts
                    .AnyAsync(a => a.Id == cmd.CashAccountId && !a.IsDeleted && a.OrganizationId == transaction.OrganizationId, ct);
                if (!accountExists) return (false, false, true, false);
            }

            if (cmd.ProjectId.HasValue)
            {
                var projectExists = await db.Projects
                    .AnyAsync(p => p.Id == cmd.ProjectId.Value && !p.IsDeleted && p.OrganizationId == transaction.OrganizationId, ct);
                if (!projectExists) return (false, false, false, true);
            }

            transaction.CashAccountId   = cmd.CashAccountId;
            transaction.Direction       = cmd.Direction;
            transaction.TransactionType = cmd.TransactionType;
            transaction.PartnerType     = cmd.PartnerType;
            transaction.PartnerId       = cmd.PartnerId;
            transaction.PartnerName     = cmd.PartnerName;
            transaction.Amount          = cmd.Amount;
            transaction.Date            = cmd.Date;
            transaction.ProjectId       = cmd.ProjectId;
            transaction.Note            = cmd.Note;
            transaction.UpdatedAt       = DateTimeOffset.UtcNow;
            transaction.UpdatedBy       = ctx.UserId ?? Guid.Empty;

            db.CashTransactions.Update(transaction);
            await db.SaveChangesAsync(ct);
            return (false, false, false, false);
        }
    }
}
