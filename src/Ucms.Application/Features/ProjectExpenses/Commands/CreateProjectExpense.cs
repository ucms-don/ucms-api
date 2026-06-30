namespace Ucms.Application.Features.ProjectExpenses.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateProjectExpense
{
    public record Command(
        Guid ProjectId, DateTimeOffset Date, string Category,
        decimal Amount, string? Description, string? PaymentMethod, string? Note, Guid CashAccountId);

    public record Result(Guid Id, decimal Amount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool ProjectNotFound, bool Forbidden, bool CashAccountNotFound, bool InsufficientBalance)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true, false, false);

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, orgId.Value, ct))
                return (null, false, false, true, false);

            if (!await CashTransactionLinker.HasSufficientBalanceAsync(db, cmd.CashAccountId, cmd.Amount, null, null, ct))
                return (null, false, false, false, true);

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var expense = new ProjectExpense
            {
                Id             = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                ProjectId      = cmd.ProjectId,
                Date           = cmd.Date,
                Category       = cmd.Category,
                Amount         = cmd.Amount,
                Description    = cmd.Description,
                PaymentMethod  = cmd.PaymentMethod,
                Note           = cmd.Note,
                IsDeleted      = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = userId, UpdatedBy = userId,
            };

            await db.ProjectExpenses.AddAsync(expense, ct);

            await CashTransactionLinker.UpsertAsync(
                db, CashTransactionSourceType.ProjectExpense, expense.Id,
                orgId.Value, cmd.CashAccountId,
                CashDirection.Out, CashTransactionType.ProjectExpense,
                FinancePartnerType.Other, null,
                cmd.Amount, cmd.Date, cmd.ProjectId, cmd.Note ?? cmd.Description,
                userId, ct);

            await db.SaveChangesAsync(ct);
            return (new Result(expense.Id, expense.Amount), false, false, false, false);
        }
    }
}
