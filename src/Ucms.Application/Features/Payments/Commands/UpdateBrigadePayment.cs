namespace Ucms.Application.Features.Payments.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateBrigadePayment
{
    public record Command(
        Guid Id,
        DateTimeOffset Date,
        decimal Amount,
        PaymentMethod PaymentMethod,
        string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, bool InsufficientBalance)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var payment = await db.BrigadePayments
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ct);

            if (payment is null) return (true, false, false);

            var orgId = payment.Project?.OrganizationId ?? Guid.Empty;
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (false, true, false);

            // Check balance for the new amount (excluding current transaction so it doesn't double-count)
            var cashTx = await db.CashTransactions
                .FirstOrDefaultAsync(t => t.SourceType == CashTransactionSourceType.BrigadePayment
                                       && t.SourceId == cmd.Id && !t.IsDeleted, ct);

            if (cashTx is not null)
            {
                if (!await CashTransactionLinker.HasSufficientBalanceAsync(
                        db, cashTx.CashAccountId, cmd.Amount,
                        CashTransactionSourceType.BrigadePayment, cmd.Id, ct))
                    return (false, false, true);

                // Update linked cash transaction
                var now    = DateTimeOffset.UtcNow;
                var userId = ctx.UserId ?? Guid.Empty;
                cashTx.Amount    = cmd.Amount;
                cashTx.Date      = cmd.Date;
                cashTx.Note      = cmd.Note;
                cashTx.UpdatedAt = now;
                cashTx.UpdatedBy = userId;
                db.CashTransactions.Update(cashTx);
            }

            payment.Date          = cmd.Date;
            payment.Amount        = cmd.Amount;
            payment.PaymentMethod = cmd.PaymentMethod;
            payment.Note          = cmd.Note;
            payment.UpdatedAt     = DateTimeOffset.UtcNow;
            payment.UpdatedBy     = ctx.UserId ?? Guid.Empty;
            db.BrigadePayments.Update(payment);

            await db.SaveChangesAsync(ct);
            return (false, false, false);
        }
    }
}
