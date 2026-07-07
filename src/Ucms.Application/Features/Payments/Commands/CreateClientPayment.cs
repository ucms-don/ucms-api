namespace Ucms.Application.Features.Payments.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateClientPayment
{
    public record Command(
        Guid ProjectId, Guid? ActId, DateTimeOffset Date,
        decimal Amount, PaymentMethod PaymentMethod, string? Note, Guid CashAccountId);

    public record Result(Guid Id, decimal Amount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(Result? Data, bool ProjectNotFound, bool Forbidden, bool CashAccountNotFound)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var project = await db.Projects
                .Where(p => p.Id == cmd.ProjectId)
                .Select(p => new { p.OrganizationId, p.CustomerId })
                .FirstOrDefaultAsync(ct);

            if (project is null) return (null, true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != project.OrganizationId) return (null, false, true, false);

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, project.OrganizationId, ct))
                return (null, false, false, true);

            var now       = DateTimeOffset.UtcNow;
            var userId    = ctx.UserId ?? Guid.Empty;
            var paymentId = Guid.NewGuid();
            var orgId     = project.OrganizationId;
            var customerId = project.CustomerId;

            await db.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ClearChangeTracker();
                await using var tx = await db.BeginTransactionAsync(ct);

                var payment = new ClientPayment
                {
                    Id            = paymentId,
                    ProjectId     = cmd.ProjectId,
                    ActId         = cmd.ActId,
                    Date          = cmd.Date,
                    Amount        = cmd.Amount,
                    PaymentMethod = cmd.PaymentMethod,
                    Note          = cmd.Note,
                    CreatedAt     = now, UpdatedAt = now,
                    CreatedBy     = userId, UpdatedBy = userId,
                };
                await db.ClientPayments.AddAsync(payment, ct);

                if (cmd.ActId.HasValue)
                {
                    var act = await db.ClientActs
                        .Include(a => a.Payments)
                        .FirstOrDefaultAsync(a => a.Id == cmd.ActId.Value, ct);
                    if (act is not null)
                    {
                        var paid = act.Payments.Sum(p => p.Amount) + cmd.Amount;
                        act.Status = paid >= act.TotalAmount
                            ? ActStatus.PaidFully
                            : paid > 0 ? ActStatus.PaidPartially : ActStatus.Issued;
                        db.ClientActs.Update(act);
                    }
                }

                // Kirim (In) — overdraft tekshiruvi yo'q
                await CashTransactionLinker.UpsertAsync(
                    db, balanceService,
                    CashTransactionSourceType.ClientPayment, paymentId,
                    orgId, cmd.CashAccountId,
                    CashDirection.In, CashTransactionType.ClientPayment,
                    FinancePartnerType.Customer, customerId,
                    cmd.Amount, cmd.Date, cmd.ProjectId, cmd.Note,
                    userId, ct);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (new Result(paymentId, cmd.Amount), false, false, false);
        }
    }
}
