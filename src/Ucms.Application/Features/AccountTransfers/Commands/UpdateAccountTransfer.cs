namespace Ucms.Application.Features.AccountTransfers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class UpdateAccountTransfer
{
    public record Command(
        Guid Id,
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount,
        decimal Commission,
        string TransferredBy,
        DateTimeOffset Date,
        string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, bool FromNotFound, bool ToNotFound, string? Error)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var transfer = await db.AccountTransfers
                .FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);

            if (transfer is null) return (true, false, false, false, null);
            if (!ctx.IsOwner && ctx.OrganizationId != transfer.OrganizationId)
                return (false, true, false, false, null);

            var fromExists = await db.CashAccounts
                .AnyAsync(a => a.Id == cmd.FromAccountId && !a.IsDeleted, ct);
            if (!fromExists) return (false, false, true, false, null);

            var toExists = await db.CashAccounts
                .AnyAsync(a => a.Id == cmd.ToAccountId && !a.IsDeleted, ct);
            if (!toExists) return (false, false, false, true, null);

            if (cmd.FromAccountId == cmd.ToAccountId)
                return (false, false, false, false, "Manba va maqsad kassa bir xil bo'lishi mumkin emas");

            if (cmd.Amount <= 0)
                return (false, false, false, false, "Summa 0 dan katta bo'lishi kerak");

            if (cmd.Commission < 0)
                return (false, false, false, false, "Komissiya manfiy bo'lishi mumkin emas");

            transfer.FromAccountId = cmd.FromAccountId;
            transfer.ToAccountId   = cmd.ToAccountId;
            transfer.Amount        = cmd.Amount;
            transfer.Commission    = cmd.Commission;
            transfer.TransferredBy = cmd.TransferredBy;
            transfer.Date          = cmd.Date;
            transfer.Note          = cmd.Note;
            transfer.UpdatedAt     = DateTimeOffset.UtcNow;
            transfer.UpdatedBy     = ctx.UserId ?? Guid.Empty;

            db.AccountTransfers.Update(transfer);
            await db.SaveChangesAsync(ct);
            return (false, false, false, false, null);
        }
    }
}
