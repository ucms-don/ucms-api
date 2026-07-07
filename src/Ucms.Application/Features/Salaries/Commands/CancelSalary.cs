namespace Ucms.Application.Features.Salaries.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class CancelSalary
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool AlreadyCancelled)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var salary = await db.Salaries
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == cmd.Id && !s.IsDeleted, ct);

            if (salary is null) return (true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != salary.OrganizationId)
                return (false, true, false);
            if (salary.IsCancelled) return (false, false, true);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var tx = await db.BeginTransactionAsync(ct);
                db.ClearChangeTracker();

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService,
                    CashTransactionSourceType.Salary, cmd.Id, ct);

                var s = await db.Salaries.AsTracking().FirstAsync(x => x.Id == cmd.Id, ct);
                s.IsCancelled = true;
                s.CancelledAt = DateTimeOffset.UtcNow;
                s.CancelledBy = ctx.UserId ?? Guid.Empty;
                s.UpdatedAt   = DateTimeOffset.UtcNow;
                s.UpdatedBy   = ctx.UserId ?? Guid.Empty;

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, false, false);
        }
    }
}
