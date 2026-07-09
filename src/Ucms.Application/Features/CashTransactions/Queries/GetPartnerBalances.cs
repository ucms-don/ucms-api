namespace Ucms.Application.Features.CashTransactions.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Extensions;
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
            var query = ctx.OrganizationId.HasValue
                ? db.CashTransactions.IncludeChilds(ctx.OrganizationId.Value)
                : db.CashTransactions.AsQueryable();

            if (q.PartnerType.HasValue)
                query = query.Where(t => t.PartnerType == q.PartnerType.Value);

            if (q.PartnerId.HasValue)
                query = query.Where(t => t.PartnerId == q.PartnerId.Value);

            // Bitta Select ichidagi bir nechta filtrlangan Sum'ni (Kirim/Chiqim alohida)
            // EF Core + Npgsql SQL'ga tarjima qila olmaydi. Shu sababli bazadan faqat
            // kerakli ustunlar (org/partner bo'yicha allaqachon filtrlangan) olinadi va
            // guruhlash/yig'indi xotirada bajariladi — hajm bitta tashkilot doirasida.
            var rows = await query
                .Select(t => new { t.PartnerType, t.PartnerId, t.Direction, t.Amount })
                .ToListAsync(ct);

            var grouped = rows
                .GroupBy(t => new { t.PartnerType, t.PartnerId })
                .Select(g =>
                {
                    var totalIn  = g.Where(t => t.Direction == CashDirection.In).Sum(t => t.Amount);
                    var totalOut = g.Where(t => t.Direction == CashDirection.Out).Sum(t => t.Amount);
                    return new Item(g.Key.PartnerType, g.Key.PartnerId,
                        totalIn, totalOut, totalIn - totalOut, g.Count());
                })
                .OrderByDescending(i => i.Balance)
                .ToList();

            return (new Result(grouped), false);
        }
    }
}
