namespace Ucms.Application.Features.CashTransactions.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetCashTransactions
{
    public record Query(
        Guid? CashAccountId, FinancePartnerType? PartnerType, Guid? PartnerId, Guid? ProjectId,
        CashDirection? Direction, CashTransactionType? TransactionType,
        DateTimeOffset? DateFrom, DateTimeOffset? DateTo,
        int Page, int Size);

    public record Item(
        Guid Id, Guid CashAccountId, string CashAccountName,
        CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? ProjectName, string? Note,
        CashTransactionSourceType? SourceType, Guid? SourceId);

    public record Result(int Total, int Page, int Size, decimal TotalIn, decimal TotalOut, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!ctx.IsOwner && !ctx.OrganizationId.HasValue) return (null, true);

            var query = db.CashTransactions;

            if (!ctx.IsOwner && ctx.OrganizationId.HasValue)
                query = query.Where(t => t.OrganizationId == ctx.OrganizationId.Value);

            if (q.CashAccountId.HasValue)
                query = query.Where(t => t.CashAccountId == q.CashAccountId.Value);

            if (q.PartnerType.HasValue)
                query = query.Where(t => t.PartnerType == q.PartnerType.Value);

            if (q.PartnerId.HasValue)
                query = query.Where(t => t.PartnerId == q.PartnerId.Value);

            if (q.ProjectId.HasValue)
                query = query.Where(t => t.ProjectId == q.ProjectId.Value);

            if (q.Direction.HasValue)
                query = query.Where(t => t.Direction == q.Direction.Value);

            if (q.TransactionType.HasValue)
                query = query.Where(t => t.TransactionType == q.TransactionType.Value);

            if (q.DateFrom.HasValue)
                query = query.Where(t => t.CreatedAt >= q.DateFrom.Value);

            if (q.DateTo.HasValue)
                query = query.Where(t => t.CreatedAt <= q.DateTo.Value);

            var total    = await query.CountAsync(ct);
            var totalIn  = await query.Where(t => t.Direction == CashDirection.In).SumAsync(t => t.Amount, ct);
            var totalOut = await query.Where(t => t.Direction == CashDirection.Out).SumAsync(t => t.Amount, ct);

            var items = await query
                .OrderByDescending(t => t.Date)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(t => new Item(
                    t.Id, t.CashAccountId, t.CashAccount!.Name,
                    t.Direction, t.TransactionType, t.PartnerType, t.PartnerId, t.PartnerName,
                    t.Amount, t.Date, t.ProjectId,
                    t.Project != null ? t.Project.Name : null, t.Note,
                    t.SourceType, t.SourceId))
                .ToListAsync(ct);

            return (new Result(total, q.Page, q.Size, totalIn, totalOut, items), false);
        }
    }
}
