namespace Ucms.Application.Features.Skus.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class CheckSkuForUsed
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Query q, CancellationToken ct)
        {
            var inIncome  = await db.IncomeItems.AnyAsync(a => a.SkuId == q.Id, ct);
            var inOutcome = await db.OutcomeItems.AnyAsync(a => a.SkuId == q.Id, ct);
            var inBalance = await db.StockSkus.AnyAsync(a => a.SkuId == q.Id && a.Amount > 0, ct);
            return inIncome || inOutcome || inBalance;
        }
    }
}
