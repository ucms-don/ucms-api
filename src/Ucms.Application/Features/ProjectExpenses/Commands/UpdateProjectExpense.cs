namespace Ucms.Application.Features.ProjectExpenses.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateProjectExpense
{
    public record Command(
        Guid ProjectId, Guid Id, DateTimeOffset Date, string Category,
        decimal Amount, string? Description, string? PaymentMethod, string? Note, Guid? CashAccountId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, bool CashAccountNotFound)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var expense = await db.ProjectExpenses
                .FirstOrDefaultAsync(e => e.Id == cmd.Id && e.ProjectId == cmd.ProjectId && !e.IsDeleted, ct);

            if (expense is null) return (true, false, false);

            if (!ctx.IsOwner && ctx.OrganizationId != expense.OrganizationId) return (false, true, false);

            if (cmd.CashAccountId.HasValue &&
                !await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId.Value, expense.OrganizationId, ct))
                return (false, false, true);

            expense.Date          = cmd.Date;
            expense.Category      = cmd.Category;
            expense.Amount        = cmd.Amount;
            expense.Description   = cmd.Description;
            expense.PaymentMethod = cmd.PaymentMethod;
            expense.Note          = cmd.Note;
            expense.UpdatedAt     = DateTimeOffset.UtcNow;
            expense.UpdatedBy     = ctx.UserId ?? Guid.Empty;

            db.ProjectExpenses.Update(expense);

            var userId = ctx.UserId ?? Guid.Empty;
            if (cmd.CashAccountId.HasValue)
            {
                await CashTransactionLinker.UpsertAsync(
                    db, CashTransactionSourceType.ProjectExpense, expense.Id,
                    expense.OrganizationId, cmd.CashAccountId.Value,
                    CashDirection.Out, CashTransactionType.ProjectExpense,
                    FinancePartnerType.Other, null,
                    cmd.Amount, cmd.Date, cmd.ProjectId, cmd.Note ?? cmd.Description,
                    userId, ct);
            }
            else
            {
                await CashTransactionLinker.RemoveAsync(db, CashTransactionSourceType.ProjectExpense, expense.Id, userId, ct);
            }

            await db.SaveChangesAsync(ct);
            return (false, false, false);
        }
    }
}
