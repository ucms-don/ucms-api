namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateSku
{
    public record Command(
        Guid Id, string SerialNumber, Guid ProductId, Guid? ManufacturerId,
        Guid MeasurementUnitId, Guid? SupplierId,
        decimal Price, decimal Amount, DateTimeOffset ExpirationDate, SkuStatus Status,
        Guid? CashAccountId);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, ICashBalanceService balanceService)
    {
        public async Task<(bool Found, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var sku = await db.Skus.FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (sku is null) return (false, null);

            var orgId  = workContext.TenantId!.Value;
            var userId = workContext.UserId ?? Guid.Empty;

            if (cmd.CashAccountId.HasValue)
            {
                if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId.Value, orgId, ct))
                    return (true, "Tanlangan kassa/bank hisobi topilmadi. / Касса/счёт не найден.");
            }

            var productName = await db.Products
                .Where(p => p.Id == cmd.ProductId)
                .Select(p => (string?)p.Name)
                .FirstOrDefaultAsync(ct);

            var totalCost = cmd.Price * cmd.Amount;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var s = await db.Skus.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
                    if (s is null) return;

                    s.SerialNumber      = cmd.SerialNumber;
                    s.Price             = cmd.Price;
                    s.Amount            = cmd.Amount;
                    s.ExpirationDate    = cmd.ExpirationDate;
                    s.ProductId         = cmd.ProductId;
                    s.ManufacturerId    = cmd.ManufacturerId;
                    s.MeasurementUnitId = cmd.MeasurementUnitId;
                    s.SupplierId        = cmd.SupplierId;
                    s.Status            = cmd.Status;

                    if (cmd.CashAccountId.HasValue)
                    {
                        await CashTransactionLinker.UpsertAsync(
                            db, balanceService,
                            CashTransactionSourceType.SkuPurchase, cmd.Id,
                            orgId, cmd.CashAccountId.Value,
                            CashDirection.Out, CashTransactionType.SupplierPayment,
                            FinancePartnerType.Supplier, cmd.SupplierId,
                            totalCost, DateTimeOffset.UtcNow, null,
                            $"Sklad: {productName} ({cmd.SerialNumber})",
                            userId, ct);
                    }
                    else
                    {
                        // Hisob tozalangan — bog'langan to'lovni bekor qilamiz
                        await CashTransactionLinker.RemoveAsync(
                            db, balanceService, CashTransactionSourceType.SkuPurchase, cmd.Id, ct);
                    }

                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException ex)
            {
                return (true, ex.Message);
            }

            return (true, null);
        }
    }
}
