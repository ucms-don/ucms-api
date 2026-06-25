namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteSkus
{
    public record Command(Guid[] Ids);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext)
    {
        public async Task<(int Deleted, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (db.IncomeItems.Any(a => cmd.Ids.Contains(a.SkuId)) ||
                db.OutcomeItems.Any(a => cmd.Ids.Contains(a.SkuId)) ||
                db.StockSkus.Any(a => cmd.Ids.Contains(a.SkuId)))
                return (0, "SKUlar boshqa jadvallarda ishlatilmoqda");

            var skus = await db.Skus.AsTracking()
                .Where(f => cmd.Ids.Contains(f.Id)).ToListAsync(ct);
            var userId = workContext.UserId ?? Guid.Empty;
            foreach (var s in skus)
            {
                s.IsDeleted = true;
                await CashTransactionLinker.RemoveAsync(
                    db, CashTransactionSourceType.SkuPurchase, s.Id, userId, ct);
            }
            await db.SaveChangesAsync(ct);
            return (skus.Count, null);
        }
    }
}
