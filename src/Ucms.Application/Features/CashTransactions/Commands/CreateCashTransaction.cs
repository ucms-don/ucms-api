namespace Ucms.Application.Features.CashTransactions.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class CreateCashTransaction
{
    public record Command(
        Guid CashAccountId, CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? Note);

    public record Result(Guid Id, decimal Amount, CashDirection Direction);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(Result? Data, bool CashAccountNotFound, bool ProjectNotFound, bool Forbidden, bool InsufficientBalance)> HandleAsync(Command cmd, CancellationToken ct)
        {
            // validation outside retry loop
            var account = await db.CashAccounts
                .Where(a => a.Id == cmd.CashAccountId)
                .Select(a => new { a.Id, a.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (account is null) return (null, true, false, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != account.OrganizationId) return (null, false, false, true, false);

            if (cmd.ProjectId.HasValue)
            {
                var projectExists = await db.Projects
                    .AnyAsync(p => p.Id == cmd.ProjectId.Value && p.OrganizationId == account.OrganizationId, ct);
                if (!projectExists) return (null, false, true, false, false);
            }

            var txId  = Guid.NewGuid();
            var now   = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;
            var orgId  = account.OrganizationId;

            Result? result = null;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var cashTx = new CashTransaction
                    {
                        Id              = txId,
                        OrganizationId  = orgId,
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

                    await db.CashTransactions.AddAsync(cashTx, ct);
                    await db.SaveChangesAsync(ct);

                    await balanceService.ApplyDeltaAsync(
                        cmd.CashAccountId, cmd.Amount, cmd.Direction,
                        allowOverdraft: false, ct: ct);

                    await tx.CommitAsync(ct);
                    result = new Result(txId, cmd.Amount, cmd.Direction);
                });
            }
            catch (InsufficientBalanceException)
            {
                return (null, false, false, false, true);
            }

            return (result, false, false, false, false);
        }
    }
}
