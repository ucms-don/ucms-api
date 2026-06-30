namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateSku
{
    public record Command(
        string? SerialNumber, Guid ProductId, Guid? ManufacturerId,
        Guid MeasurementUnitId, Guid? SupplierId,
        decimal Price, decimal Amount, DateTimeOffset ExpirationDate, SkuStatus Status,
        Guid? CashAccountId, Guid? StockId);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, ISkuSerialNumberGenerator serialGenerator)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            // SerialNumber qo'lda kiritilmagan bo'lsa, mahsulot (Product.Code) asosida
            // avtomatik va noyob seriya raqami generatsiya qilinadi.
            var serialNumber = string.IsNullOrWhiteSpace(cmd.SerialNumber)
                ? await serialGenerator.GenerateAsync(cmd.ProductId, ct)
                : cmd.SerialNumber.Trim();

            if (await db.Skus.AnyAsync(f => f.SerialNumber == serialNumber, ct))
                return (null, $"'{serialNumber}' seriya raqami allaqachon mavjud");

            var orgId  = workContext.TenantId!.Value;
            var userId = workContext.UserId ?? Guid.Empty;
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == cmd.ProductId, ct);

            // Skladga material kiritilayotganda to'lov qilinadigan kassa/bank ko'rsatilgan bo'lsa,
            // o'sha hisobdan Narx × Miqdor summasi yechiladi (chiqim — yetkazib beruvchiga to'lov).
            var totalCost = cmd.Price * cmd.Amount;
            if (cmd.CashAccountId.HasValue)
            {
                if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId.Value, orgId, ct))
                    return (null, "Tanlangan kassa/bank hisobi topilmadi. / Касса/счёт не найден.");

                if (!await CashTransactionLinker.HasSufficientBalanceAsync(db, cmd.CashAccountId.Value, totalCost, null, null, ct))
                    return (null, "Kassada mablag' yetarli emas. / Недостаточно средств на счёте.");
            }

            var sku = new Sku
            {
                Id = Guid.NewGuid(),
                SerialNumber = serialNumber, Price = cmd.Price, Amount = cmd.Amount,
                ExpirationDate = cmd.ExpirationDate, ProductId = cmd.ProductId,
                ManufacturerId = cmd.ManufacturerId, MeasurementUnitId = cmd.MeasurementUnitId,
                SupplierId = cmd.SupplierId, Status = cmd.Status
            };
            db.OrganizationSkus.Add(new OrganizationSku { OrganizationId = orgId, Sku = sku });

            if (cmd.CashAccountId.HasValue)
            {
                await CashTransactionLinker.UpsertAsync(
                    db, CashTransactionSourceType.SkuPurchase, sku.Id,
                    orgId, cmd.CashAccountId.Value,
                    CashDirection.Out, CashTransactionType.SupplierPayment,
                    FinancePartnerType.Supplier, cmd.SupplierId,
                    totalCost, DateTimeOffset.UtcNow, null, $"Sklad: {product?.Name} ({serialNumber})",
                    userId, ct);
            }

            // Skladga qoldiq qo'shish: tovar qabul qilinganda tanlangan (yoki asosiy/Central) skladga
            // StockSku balansi yoziladi. Shunda "Расход товара" bu mahsulotni ombor qoldig'ida ko'radi
            // va chiqim qila oladi (aks holda doim "Недостаточно" xatoligi chiqardi).
            var targetStockId = cmd.StockId
                ?? await db.Stocks
                    .Where(s => s.OrganizationId == orgId && s.StockCategory == StockCategory.Central)
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefaultAsync(ct)
                ?? await db.Stocks
                    .Where(s => s.OrganizationId == orgId)
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefaultAsync(ct);

            if (targetStockId.HasValue && cmd.Amount > 0)
            {
                var mu = await db.MeasurementUnits.FirstOrDefaultAsync(m => m.Id == cmd.MeasurementUnitId, ct);
                var multiplier = mu?.Multiplier ?? 1m;
                // Qoldiq bazaviy birlikda (Multiplier == 1) saqlanadi — chiqim validatsiyasi ham shunga tayanadi.
                var baseMuId = mu == null
                    ? cmd.MeasurementUnitId
                    : (await db.MeasurementUnits.FirstOrDefaultAsync(m => m.Type == mu.Type && m.Multiplier == 1m, ct))?.Id ?? cmd.MeasurementUnitId;
                var baseAmount = cmd.Amount * multiplier;

                db.StockSkus.Add(new StockSku
                {
                    SkuId = sku.Id,
                    StockId = targetStockId.Value,
                    Amount = baseAmount,
                    MeasurementUnitId = baseMuId,
                });
                db.StockBalanceRegisters.Add(new StockBalanceRegister
                {
                    StockId = targetStockId.Value,
                    SkuId = sku.Id,
                    ProductId = cmd.ProductId,
                    MeasurementUnitId = baseMuId,
                    PreviousAmount = 0,
                    CurrentAmount = baseAmount,
                    VariableAmount = baseAmount,
                    Date = DateTimeOffset.UtcNow,
                    Type = (int)IncomeType.External,
                });
            }

            await db.SaveChangesAsync(ct);
            return (sku.Id, null);
        }
    }
}
