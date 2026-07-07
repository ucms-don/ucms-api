namespace Ucms.Application.Features.Salaries.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteSalary
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var salary = await db.Salaries.FindAsync([cmd.Id], ct);
            if (salary is null || salary.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != salary.OrganizationId) return (false, true);

            var userId = ctx.UserId ?? Guid.Empty;

            await db.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ClearChangeTracker();
                await using var tx = await db.BeginTransactionAsync(ct);

                var s = await db.Salaries.FindAsync([cmd.Id], ct);
                if (s is null || s.IsDeleted) return;

                s.IsDeleted = true;
                s.UpdatedAt = DateTimeOffset.UtcNow;
                s.UpdatedBy = userId;
                db.Salaries.Update(s);

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService, CashTransactionSourceType.Salary, cmd.Id, ct);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, false);
        }
    }
}
