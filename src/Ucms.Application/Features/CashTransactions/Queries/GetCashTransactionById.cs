namespace Ucms.Application.Features.CashTransactions.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetCashTransactionById
{
    public record Query(Guid Id);

    public record CashTransactionDetailDto(
        Guid Id, Guid CashAccountId, string CashAccountName,
        CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? ProjectName, string? Note,
        CashTransactionSourceType? SourceType, Guid? SourceId,
        Guid OrganizationId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(CashTransactionDetailDto? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var transaction = await db.CashTransactions
                .Where(t => t.Id == q.Id)
                .Select(t => new CashTransactionDetailDto(
                    t.Id, t.CashAccountId, t.CashAccount!.Name,
                    t.Direction, t.TransactionType, t.PartnerType, t.PartnerId, t.PartnerName,
                    t.Amount, t.Date, t.ProjectId,
                    t.Project != null ? t.Project.Name : null, t.Note,
                    t.SourceType, t.SourceId,
                    t.OrganizationId, t.CreatedAt, t.UpdatedAt))
                .FirstOrDefaultAsync(ct);

            if (transaction is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != transaction.OrganizationId) return (null, true);
            return (transaction, false);
        }
    }
}
