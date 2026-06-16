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
        Guid ProjectId,
        Guid? ActId,
        DateTimeOffset Date,
        decimal Amount,
        PaymentMethod PaymentMethod,
        string? Note,
        Guid? CashAccountId);

    public record Result(Guid Id, decimal Amount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool ProjectNotFound, bool Forbidden, bool CashAccountNotFound)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var project = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => new { p.OrganizationId, p.CustomerId })
                .FirstOrDefaultAsync(ct);

            if (project is null) return (null, true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != project.OrganizationId) return (null, false, true, false);

            if (cmd.CashAccountId.HasValue &&
                !await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId.Value, project.OrganizationId, ct))
                return (null, false, false, true);

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var payment = new ClientPayment
            {
                Id            = Guid.NewGuid(),
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
                await UpdateActStatusAsync(cmd.ActId.Value, ct);

            if (cmd.CashAccountId.HasValue)
            {
                await CashTransactionLinker.UpsertAsync(
                    db, CashTransactionSourceType.ClientPayment, payment.Id,
                    project.OrganizationId, cmd.CashAccountId.Value,
                    CashDirection.In, CashTransactionType.ClientPayment,
                    FinancePartnerType.Customer, project.CustomerId,
                    cmd.Amount, cmd.Date, cmd.ProjectId, cmd.Note,
                    userId, ct);
            }

            await db.SaveChangesAsync(ct);
            return (new Result(payment.Id, payment.Amount), false, false, false);
        }

        private async Task UpdateActStatusAsync(Guid actId, CancellationToken ct)
        {
            var act = await db.ClientActs
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.Id == actId, ct);

            if (act is null) return;

            var paid = act.Payments.Sum(p => p.Amount);
            act.Status = paid >= act.TotalAmount
                ? ActStatus.PaidFully
                : paid > 0 ? ActStatus.PaidPartially : ActStatus.Issued;

            db.ClientActs.Update(act);
        }
    }
}
