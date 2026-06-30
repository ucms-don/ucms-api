namespace Ucms.Application.Features.Products.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteProducts
{
    public record Command(Guid[] Ids);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(int Deleted, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (db.Skus.Any(a => cmd.Ids.Contains(a.ProductId)) ||
                db.StockDemandItems.Any(a => cmd.Ids.Contains(a.ProductId)))
                return (0, "Mahsulotlar boshqa jadvallarda ishlatilmoqda");

            var products = await db.Products.AsTracking()
                .Where(f => cmd.Ids.Contains(f.Id)).ToListAsync(ct);
            foreach (var p in products) p.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (products.Count, null);
        }
    }
}
