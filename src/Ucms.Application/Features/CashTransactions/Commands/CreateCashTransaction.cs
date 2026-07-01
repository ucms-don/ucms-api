namespace Ucms.Application.Features.CashTransactions.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateCashTransaction
{
    public record Command(
        Guid CashAccountId, CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? Note);

    public record Result(Guid Id, decimal Amount, CashDirection Direction);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool CashAccountNotFound, bool ProjectNotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = ctx.IsOwner ? null : ctx.OrganizationId;

            var account = await db.CashAccounts
                .Where(a => a.Id == cmd.CashAccountId && !a.IsDeleted)
                .Select(a => new { a.Id, a.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (account is null) return (null, true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != account.OrganizationId) return (null, false, false, true);

            if (cmd.ProjectId.HasValue)
            {
                var projectExists = await db.Projects
                    .AnyAsync(p => p.Id == cmd.ProjectId.Value && !p.IsDeleted && p.OrganizationId == account.OrganizationId, ct);
                if (!projectExists) return (null, false, true, false);
            }

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var transaction = new CashTransaction
            {
                Id              = Guid.NewGuid(),
                OrganizationId  = account.OrganizationId,
                CashAccountId   = cmd.CashAccountId,
                Direction       = cmd.Direction,
                TransactionType = cmd.TransactionType,
                PartnerType     = cmd.PartnerType,
                PartnerId       = cmd.PartnerId,
                PartnerName     = cmd.PartnerName,
                Amount          = cmd.Amount,
                Date            = cmd.Date,
                ProjectId       = cmd.ProjectId,
                Note            = cmd.Note,
                CreatedAt       = now, UpdatedAt = now,
                CreatedBy       = userId, UpdatedBy = userId,
            };

            await db.CashTransactions.AddAsync(transaction, ct);
            await db.SaveChangesAsync(ct);
            return (new Result(transaction.Id, transaction.Amount, transaction.Direction), false, false, false);
        }
    }
}
