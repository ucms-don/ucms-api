namespace Ucms.Application.Features.ProjectExpenses.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateProjectExpense
{
    public record Command(
        Guid ProjectId, Guid Id, DateTimeOffset Date, string Category,
        decimal Amount, string? Description, string? PaymentMethod, string? Note, Guid CashAccountId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool CashAccountNotFound, bool InsufficientBalance)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var expense = await db.ProjectExpenses
                .FirstOrDefaultAsync(e => e.Id == cmd.Id && e.ProjectId == cmd.ProjectId, ct);

            if (expense is null) return (true, false, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != expense.OrganizationId) return (false, true, false, false);

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, expense.OrganizationId, ct))
                return (false, false, true, false);

            var userId = ctx.UserId ?? Guid.Empty;
            var orgId  = expense.OrganizationId;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var e = await db.ProjectExpenses
                        .FirstOrDefaultAsync(x => x.Id == cmd.Id && x.ProjectId == cmd.ProjectId, ct);
                    if (e is null) return;

                    e.Date          = cmd.Date;
                    e.Category      = cmd.Category;
                    e.Amount        = cmd.Amount;
                    e.Description   = cmd.Description;
                    e.PaymentMethod = cmd.PaymentMethod;
                    e.Note          = cmd.Note;
                    e.UpdatedAt     = DateTimeOffset.UtcNow;
                    e.UpdatedBy     = userId;
                    db.ProjectExpenses.Update(e);

                    await CashTransactionLinker.UpsertAsync(
                        db, balanceService,
                        CashTransactionSourceType.ProjectExpense, cmd.Id,
                        orgId, cmd.CashAccountId,
                        CashDirection.Out, CashTransactionType.ProjectExpense,
                        FinancePartnerType.Other, null,
                        cmd.Amount, cmd.Date, cmd.ProjectId, cmd.Note ?? cmd.Description,
                        userId, ct);

                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException)
            {
                return (false, false, false, true);
            }

            return (false, false, false, false);
        }
    }
}
