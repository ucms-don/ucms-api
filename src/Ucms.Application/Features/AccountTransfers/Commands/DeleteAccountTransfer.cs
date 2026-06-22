namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteAccountTransfer
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var transfer = await db.AccountTransfers
                .FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);

            if (transfer is null) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != transfer.OrganizationId)
                return (false, true);

            transfer.IsDeleted  = true;
            transfer.UpdatedAt  = DateTimeOffset.UtcNow;
            transfer.UpdatedBy  = ctx.UserId ?? Guid.Empty;

            db.AccountTransfers.Update(transfer);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
