namespace Ucms.Application.Features.CashTransactions.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

/// <summary>
/// Har bir partner (Supplier/Owner/Lender/Other) bo'yicha kassa harakatlari asosida
/// hisoblangan balans. Balans = Kirimlar yig'indisi - Chiqimlar yig'indisi.
/// Masalan Lender uchun: olingan kreditlar (In) - qaytarilganlari (Out) = qolgan qarz.
/// </summary>
public static class GetPartnerBalances
{
    public record Query(FinancePartnerType? PartnerType, Guid? PartnerId);

    public record Item(
        FinancePartnerType PartnerType, Guid? PartnerId,
        decimal TotalIn, decimal TotalOut, decimal Balance, int TransactionsCount);

    public record Result(List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!ctx.IsOwner && !ctx.OrganizationId.HasValue) return (null, true);

            var query = db.CashTransactions.Where(t => !t.IsDeleted);

            if (!ctx.IsOwner && ctx.OrganizationId.HasValue)
                query = query.Where(t => t.OrganizationId == ctx.OrganizationId.Value);

            if (q.PartnerType.HasValue)
                query = query.Where(t => t.PartnerType == q.PartnerType.Value);

            if (q.PartnerId.HasValue)
                query = query.Where(t => t.PartnerId == q.PartnerId.Value);

            var grouped = await query
                .GroupBy(t => new { t.PartnerType, t.PartnerId })
                .Select(g => new Item(
                    g.Key.PartnerType, g.Key.PartnerId,
                    g.Where(t => t.Direction == CashDirection.In).Sum(t => t.Amount),
                    g.Where(t => t.Direction == CashDirection.Out).Sum(t => t.Amount),
                    g.Sum(t => t.Direction == CashDirection.In ? t.Amount : -t.Amount),
                    g.Count()))
                .OrderByDescending(i => i.Balance)
                .ToListAsync(ct);

            return (new Result(grouped), false);
        }
    }
}
