namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteSku
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var sku = await db.Skus.FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
            if (sku is null) return (true, null);

            if (await db.IncomeItems.AnyAsync(a => a.SkuId == cmd.Id, ct) ||
                await db.OutcomeItems.AnyAsync(a => a.SkuId == cmd.Id, ct) ||
                await db.StockSkus.AnyAsync(a => a.SkuId == cmd.Id, ct))
                return (false, "SKU boshqa jadvallarda ishlatilmoqda");

            var userId = workContext.UserId ?? Guid.Empty;

            await db.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                db.ClearChangeTracker();
                await using var tx = await db.BeginTransactionAsync(ct);

                var s = await db.Skus.AsTracking().FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
                if (s is null) return;

                s.IsDeleted = true;

                await CashTransactionLinker.RemoveAsync(
                    db, balanceService, CashTransactionSourceType.SkuPurchase, cmd.Id, ct);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            return (false, null);
        }
    }
}
