namespace Ucms.Application.Features.StockSkus.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class CheckSkuAmount
{
    public record Query(Guid SkuId, Guid StockId, Guid MeasurementUnitId, decimal Amount);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Query q, CancellationToken ct)
        {
            var mu = await db.MeasurementUnits.FirstOrDefaultAsync(f => f.Id == q.MeasurementUnitId, ct);
            if (mu is null) return false;
            var amount = q.Amount * mu.Multiplier;
            return await db.StockSkus.AnyAsync(f =>
                f.StockId == q.StockId && f.SkuId == q.SkuId && f.Amount >= amount, ct);
        }
    }
}
