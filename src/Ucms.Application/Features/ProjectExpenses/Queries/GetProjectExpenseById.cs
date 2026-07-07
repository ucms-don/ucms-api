namespace Ucms.Application.Features.ProjectExpenses.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetProjectExpenseById
{
    public record Query(Guid ProjectId, Guid Id);

    public record ExpenseDetailDto(
        Guid Id, Guid ProjectId, DateTimeOffset Date, string Category, decimal Amount,
        string? Description, string? PaymentMethod, string? Note,
        Guid OrganizationId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(ExpenseDetailDto? Data, bool NotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var expense = await db.ProjectExpenses
                .Where(e => e.Id == q.Id && e.ProjectId == q.ProjectId)
                .Select(e => new ExpenseDetailDto(
                    e.Id, e.ProjectId, e.Date, e.Category, e.Amount,
                    e.Description, e.PaymentMethod, e.Note,
                    e.OrganizationId, e.CreatedAt, e.UpdatedAt))
                .FirstOrDefaultAsync(ct);

            if (expense is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != expense.OrganizationId) return (null, false, true);
            return (expense, false, false);
        }
    }
}
