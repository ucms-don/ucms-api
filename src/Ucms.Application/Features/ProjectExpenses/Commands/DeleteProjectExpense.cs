namespace Ucms.Application.Features.ProjectExpenses.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteProjectExpense
{
    public record Command(Guid ProjectId, Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var expense = await db.ProjectExpenses
                .FirstOrDefaultAsync(e => e.Id == cmd.Id && e.ProjectId == cmd.ProjectId, ct);

            if (expense is null) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != expense.OrganizationId) return (false, true);

            var userId = ctx.UserId ?? Guid.Empty;

            await db.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ClearChangeTracker();
                await using var tx = await db.BeginTransactionAsync(ct);

                var e = await db.ProjectExpenses
                    .FirstOrDefaultAsync(x => x.Id == cmd.Id && x.ProjectId == cmd.ProjectId, ct);
                if (e is null) return;

                e.IsDeleted = true;
                e.UpdatedAt = DateTimeOffset.UtcNow;
                e.UpdatedBy = userId;
                db.ProjectExpenses.Update(e);

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService, CashTransactionSourceType.ProjectExpense, cmd.Id, userId, ct);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, false);
        }
    }
}
