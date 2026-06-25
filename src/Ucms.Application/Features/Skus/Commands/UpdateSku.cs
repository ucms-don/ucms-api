namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateSku
{
    public record Command(
        Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        string SerialNumber, Guid ProductId, Guid? ManufacturerId,
        Guid MeasurementUnitId, Guid? SupplierId,
        decimal Price, decimal Amount, DateTimeOffset ExpirationDate, SkuStatus Status,
        Guid? CashAccountId);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext)
    {
        public async Task<(bool Found, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var sku = await db.Skus.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (sku is null) return (false, null);

            var orgId  = workContext.TenantId!.Value;
            var userId = workContext.UserId ?? Guid.Empty;
            var totalCost = cmd.Price * cmd.Amount;

            // Kassa/bank ko'rsatilgan bo'lsa, bog'langan chiqim (yetkazib beruvchiga to'lov)
            // yangi Narx × Miqdor bo'yicha qayta sinxronlanadi. Balansni tekshirishda shu Sku'ning
            // eski tranzaksiyasi hisobga olinmaydi (exclude). Kassa tozalansa (null) — to'lov bekor qilinadi.
            if (cmd.CashAccountId.HasValue)
            {
                if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId.Value, orgId, ct))
                    return (true, "Tanlangan kassa/bank hisobi topilmadi. / Касса/счёт не найден.");

                if (!await CashTransactionLinker.HasSufficientBalanceAsync(
                        db, cmd.CashAccountId.Value, totalCost,
                        CashTransactionSourceType.SkuPurchase, sku.Id, ct))
                    return (true, "Kassada mablag' yetarli emas. / Недостаточно средств на счёте.");
            }

            sku.Name = cmd.Name; sku.NameRu = cmd.NameRu; sku.NameEn = cmd.NameEn; sku.NameKa = cmd.NameKa;
            sku.SerialNumber = cmd.SerialNumber; sku.Price = cmd.Price; sku.Amount = cmd.Amount;
            sku.ExpirationDate = cmd.ExpirationDate; sku.ProductId = cmd.ProductId;
            sku.ManufacturerId = cmd.ManufacturerId; sku.MeasurementUnitId = cmd.MeasurementUnitId;
            sku.SupplierId = cmd.SupplierId; sku.Status = cmd.Status;

            if (cmd.CashAccountId.HasValue)
            {
                await CashTransactionLinker.UpsertAsync(
                    db, CashTransactionSourceType.SkuPurchase, sku.Id,
                    orgId, cmd.CashAccountId.Value,
                    CashDirection.Out, CashTransactionType.SupplierPayment,
                    FinancePartnerType.Supplier, cmd.SupplierId,
                    totalCost, DateTimeOffset.UtcNow, null, $"Sklad: {cmd.Name} ({cmd.SerialNumber})",
                    userId, ct);
            }
            else
            {
                // Hisob tozalangan bo'lsa, bog'langan to'lovni bekor qilamiz (balans qaytadi).
                await CashTransactionLinker.RemoveAsync(
                    db, CashTransactionSourceType.SkuPurchase, sku.Id, userId, ct);
            }

            await db.SaveChangesAsync(ct);
            return (true, null);
        }
    }
}
