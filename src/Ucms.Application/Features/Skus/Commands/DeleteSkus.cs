namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteSkus
{
    public record Command(Guid[] Ids);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, ICashBalanceService balanceService)
    {
        public async Task<(int Deleted, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.IncomeItems.AnyAsync(a => cmd.Ids.Contains(a.SkuId), ct) ||
                await db.OutcomeItems.AnyAsync(a => cmd.Ids.Contains(a.SkuId), ct) ||
                await db.StockSkus.AnyAsync(a => cmd.Ids.Contains(a.SkuId), ct))
                return (0, "SKUlar boshqa jadvallarda ishlatilmoqda");

            var count  = await db.Skus.CountAsync(f => cmd.Ids.Contains(f.Id), ct);
            var userId = workContext.UserId ?? Guid.Empty;

            await db.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ClearChangeTracker();
                await using var tx = await db.BeginTransactionAsync(ct);

                var skus = await db.Skus.AsTracking()
                    .Where(f => cmd.Ids.Contains(f.Id))
                    .ToListAsync(ct);

                foreach (var s in skus)
                {
                    s.IsDeleted = true;
                    await CashTransactionLinker.RemoveAsync(
                        db, balanceService, CashTransactionSourceType.SkuPurchase, s.Id, ct);
                }

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (count, null);
        }
    }
}
