namespace Ucms.Application.Features.CashAccounts.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteCashAccount
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var account = await db.CashAccounts.FindAsync([cmd.Id], ct);
            if (account is null || account.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != account.OrganizationId) return (false, true);

            account.IsDeleted = true;
            account.UpdatedAt = DateTimeOffset.UtcNow;
            account.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.CashAccounts.Update(account);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
