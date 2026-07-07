namespace Ucms.Application.Features.Payments.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class CreateBrigadePayment
{
    public record Command(
        Guid ProjectId, Guid BrigadeId, DateTimeOffset Date,
        decimal Amount, PaymentMethod PaymentMethod,
        Guid[] WorkLogIds, string? Note, Guid CashAccountId);

    public record Result(Guid Id, decimal Amount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(Result? Data, bool ProjectNotFound, bool Forbidden, bool CashAccountNotFound, bool InsufficientBalance)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true, false, false);

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, orgId.Value, ct))
                return (null, false, false, true, false);

            var now       = DateTimeOffset.UtcNow;
            var userId    = ctx.UserId ?? Guid.Empty;
            var paymentId = Guid.NewGuid();

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var payment = new BrigadePayment
                    {
                        Id            = paymentId,
                        ProjectId     = cmd.ProjectId,
                        BrigadeId     = cmd.BrigadeId,
                        Date          = cmd.Date,
                        Amount        = cmd.Amount,
                        PaymentMethod = cmd.PaymentMethod,
                        Note          = cmd.Note,
                        CreatedAt     = now, UpdatedAt = now,
                        CreatedBy     = userId, UpdatedBy = userId,
                    };
                    await db.BrigadePayments.AddAsync(payment, ct);

                    if (cmd.WorkLogIds.Length > 0)
                    {
                        var workLogs = await db.WorkLogs
                            .Where(w => cmd.WorkLogIds.Contains(w.Id)
                                     && w.ProjectId == cmd.ProjectId
                                     && w.BrigadeId == cmd.BrigadeId
                                     && w.Status == WorkLogStatus.Confirmed)
                            .ToListAsync(ct);

                        foreach (var wl in workLogs)
                        {
                            wl.Status           = WorkLogStatus.Paid;
                            wl.BrigadePaymentId = paymentId;
                            wl.UpdatedAt        = now;
                            wl.UpdatedBy        = userId;
                            db.WorkLogs.Update(wl);
                        }
                    }

                    await CashTransactionLinker.UpsertAsync(
                        db, balanceService,
                        CashTransactionSourceType.BrigadePayment, paymentId,
                        orgId.Value, cmd.CashAccountId,
                        CashDirection.Out, CashTransactionType.BrigadePayment,
                        FinancePartnerType.Brigade, cmd.BrigadeId,
                        cmd.Amount, cmd.Date, cmd.ProjectId, cmd.Note,
                        userId, ct);

                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException)
            {
                return (null, false, false, false, true);
            }

            return (new Result(paymentId, cmd.Amount), false, false, false, false);
        }
    }
}
