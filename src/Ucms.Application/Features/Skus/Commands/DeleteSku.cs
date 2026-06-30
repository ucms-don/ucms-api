namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteSku
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var sku = await db.Skus.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
            if (sku is null) return (true, null);

            if (db.IncomeItems.Any(a => a.SkuId == cmd.Id) ||
                db.OutcomeItems.Any(a => a.SkuId == cmd.Id) ||
                db.StockSkus.Any(a => a.SkuId == cmd.Id))
                return (false, "SKU boshqa jadvallarda ishlatilmoqda");

            sku.IsDeleted = true;
            // Skladdan o'chirilganda shu materialga to'lov sifatida yaratilgan chiqimni ham bekor qilamiz.
            await CashTransactionLinker.RemoveAsync(
                db, CashTransactionSourceType.SkuPurchase, sku.Id, workContext.UserId ?? Guid.Empty, ct);
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
