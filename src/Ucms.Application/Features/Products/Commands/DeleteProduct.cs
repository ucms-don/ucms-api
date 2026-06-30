namespace Ucms.Application.Features.Products.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteProduct
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var product = await db.Products.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
            if (product is null) return (true, null);

            if (db.Skus.Any(a => a.ProductId == cmd.Id) || db.StockDemandItems.Any(a => a.ProductId == cmd.Id))
                return (false, "Mahsulot boshqa jadvallarda ishlatilmoqda");

            product.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
