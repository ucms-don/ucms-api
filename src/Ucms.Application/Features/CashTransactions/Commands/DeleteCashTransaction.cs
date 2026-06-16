namespace Ucms.Application.Features.CashTransactions.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteCashTransaction
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var transaction = await db.CashTransactions.FindAsync([cmd.Id], ct);
            if (transaction is null || transaction.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != transaction.OrganizationId) return (false, true);

            transaction.IsDeleted = true;
            transaction.UpdatedAt = DateTimeOffset.UtcNow;
            transaction.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.CashTransactions.Update(transaction);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
