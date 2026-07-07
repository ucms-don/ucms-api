namespace Ucms.Application.Features.ProjectExpenses.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class CancelProjectExpense
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool AlreadyCancelled)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var expense = await db.ProjectExpenses
                .AsTracking()
                .FirstOrDefaultAsync(e => e.Id == cmd.Id && !e.IsDeleted, ct);

            if (expense is null) return (true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != expense.OrganizationId)
                return (false, true, false);
            if (expense.IsCancelled) return (false, false, true);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var tx = await db.BeginTransactionAsync(ct);
                db.ClearChangeTracker();

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService,
                    CashTransactionSourceType.ProjectExpense, cmd.Id, ct);

                var e = await db.ProjectExpenses.AsTracking().FirstAsync(x => x.Id == cmd.Id, ct);
                e.IsCancelled = true;
                e.CancelledAt = DateTimeOffset.UtcNow;
                e.CancelledBy = ctx.UserId ?? Guid.Empty;
                e.UpdatedAt   = DateTimeOffset.UtcNow;
                e.UpdatedBy   = ctx.UserId ?? Guid.Empty;

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, false, false);
        }
    }
}
