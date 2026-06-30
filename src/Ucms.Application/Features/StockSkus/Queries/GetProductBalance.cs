namespace Ucms.Application.Features.StockSkus.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class GetProductBalance
{
    public record Query(Guid StockId, Guid ProductId, Guid MeasurementUnitId);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<decimal> HandleAsync(Query q, CancellationToken ct)
        {
            var balance = await db.StockSkus.FirstOrDefaultAsync(w =>
                w.StockId == q.StockId && w.MeasurementUnitId == q.MeasurementUnitId &&
                w.Sku!.ProductId == q.ProductId, ct);
            return balance?.Amount ?? 0;
        }
    }
}
