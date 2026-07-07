namespace Ucms.Application.Features.Payments.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class CancelClientPayment
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool AlreadyCancelled)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var payment = await db.ClientPayments
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ct);

            if (payment is null) return (true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != await db.Projects
                    .Where(p => p.Id == payment.ProjectId)
                    .Select(p => (Guid?)p.OrganizationId)
                    .FirstOrDefaultAsync(ct))
                return (false, true, false);
            if (payment.IsCancelled) return (false, false, true);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var tx = await db.BeginTransactionAsync(ct);
                db.ClearChangeTracker();

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService,
                    CashTransactionSourceType.ClientPayment, cmd.Id, ct);

                var p = await db.ClientPayments.AsTracking().FirstAsync(x => x.Id == cmd.Id, ct);
                p.IsCancelled = true;
                p.CancelledAt = DateTimeOffset.UtcNow;
                p.CancelledBy = ctx.UserId ?? Guid.Empty;
                p.UpdatedAt   = DateTimeOffset.UtcNow;
                p.UpdatedBy   = ctx.UserId ?? Guid.Empty;

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, false, false);
        }
    }
}
