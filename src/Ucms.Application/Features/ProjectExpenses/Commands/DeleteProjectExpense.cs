namespace Ucms.Application.Features.ProjectExpenses.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteProjectExpense
{
    public record Command(Guid ProjectId, Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var expense = await db.ProjectExpenses
                .FirstOrDefaultAsync(e => e.Id == cmd.Id && e.ProjectId == cmd.ProjectId && !e.IsDeleted, ct);

            if (expense is null) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != expense.OrganizationId) return (false, true);

            expense.IsDeleted = true;
            expense.UpdatedAt = DateTimeOffset.UtcNow;
            expense.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.ProjectExpenses.Update(expense);

            await CashTransactionLinker.RemoveAsync(db, CashTransactionSourceType.ProjectExpense, expense.Id, expense.UpdatedBy, ct);

            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
