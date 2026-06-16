namespace Ucms.Application.Features.CashAccounts.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateCashAccount
{
    public record Command(Guid Id, string Name, CashAccountType Type, string? Notes, bool IsActive);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var account = await db.CashAccounts.FindAsync([cmd.Id], ct);
            if (account is null || account.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != account.OrganizationId) return (false, true);

            account.Name      = cmd.Name;
            account.Type      = cmd.Type;
            account.Notes     = cmd.Notes;
            account.IsActive  = cmd.IsActive;
            account.UpdatedAt = DateTimeOffset.UtcNow;
            account.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.CashAccounts.Update(account);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
