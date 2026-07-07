namespace Ucms.Application.Features.Payments.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateBrigadePayment
{
    public record Command(
        Guid Id, DateTimeOffset Date, decimal Amount, PaymentMethod PaymentMethod, string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, bool InsufficientBalance)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var payment = await db.BrigadePayments
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ct);

            if (payment is null) return (true, false, false);

            var orgId = payment.Project?.OrganizationId ?? Guid.Empty;
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (false, true, false);

            // CashAccountId va Partner bilgilerini mavjud CashTransaction dan olamiz
            var cashTx = await db.CashTransactions
                .FirstOrDefaultAsync(t =>
                    t.SourceType == CashTransactionSourceType.BrigadePayment
                    && t.SourceId == cmd.Id, ct);

            var userId     = ctx.UserId ?? Guid.Empty;
            var brigadeId  = payment.BrigadeId;
            var projectId  = payment.ProjectId;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var p = await db.BrigadePayments
                        .Include(x => x.Project)
                        .FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
                    if (p is null) return;

                    p.Date          = cmd.Date;
                    p.Amount        = cmd.Amount;
                    p.PaymentMethod = cmd.PaymentMethod;
                    p.Note          = cmd.Note;
                    p.UpdatedAt     = DateTimeOffset.UtcNow;
                    p.UpdatedBy     = userId;
                    db.BrigadePayments.Update(p);

                    if (cashTx is not null)
                    {
                        await CashTransactionLinker.UpsertAsync(
                            db, balanceService,
                            CashTransactionSourceType.BrigadePayment, cmd.Id,
                            orgId, cashTx.CashAccountId,
                            CashDirection.Out, CashTransactionType.BrigadePayment,
                            FinancePartnerType.Brigade, brigadeId,
                            cmd.Amount, cmd.Date, projectId, cmd.Note,
                            userId, ct);
                    }

                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException)
            {
                return (false, false, true);
            }

            return (false, false, false);
        }
    }
}
