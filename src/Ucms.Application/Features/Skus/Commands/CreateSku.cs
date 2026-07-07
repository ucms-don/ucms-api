namespace Ucms.Application.Features.Skus.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class CreateSku
{
    public record Command(
        string? SerialNumber, Guid ProductId, Guid? ManufacturerId,
        Guid MeasurementUnitId, Guid? SupplierId,
        decimal Price, decimal Amount, DateTimeOffset ExpirationDate, SkuStatus Status,
        Guid? CashAccountId, Guid? StockId);

    public sealed class Handler(
        IUcmsDbContext db, IWorkContext workContext,
        ISkuSerialNumberGenerator serialGenerator,
        ICashBalanceService balanceService)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var serialNumber = string.IsNullOrWhiteSpace(cmd.SerialNumber)
                ? await serialGenerator.GenerateAsync(cmd.ProductId, ct)
                : cmd.SerialNumber.Trim();

            if (await db.Skus.AnyAsync(f => f.SerialNumber == serialNumber, ct))
                return (null, $"'{serialNumber}' seriya raqami allaqachon mavjud");

            var orgId  = workContext.TenantId!.Value;
            var userId = workContext.UserId ?? Guid.Empty;

            if (cmd.CashAccountId.HasValue)
            {
                if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId.Value, orgId, ct))
                    return (null, "Tanlangan kassa/bank hisobi topilmadi. / Касса/счёт не найден.");
            }

            var targetStockId = cmd.StockId
                ?? await db.Stocks
                    .Where(s => s.OrganizationId == orgId && s.StockCategory == StockCategory.Central)
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefaultAsync(ct)
                ?? await db.Stocks
                    .Where(s => s.OrganizationId == orgId)
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefaultAsync(ct);

            var productName = await db.Products
                .Where(p => p.Id == cmd.ProductId)
                .Select(p => (string?)p.Name)
                .FirstOrDefaultAsync(ct);

            MeasurementUnit? mu = null;
            Guid? baseMuId      = null;
            decimal multiplier  = 1m;
            decimal baseAmount  = cmd.Amount;

            if (cmd.Amount > 0 && targetStockId.HasValue)
            {
                mu         = await db.MeasurementUnits.FirstOrDefaultAsync(m => m.Id == cmd.MeasurementUnitId, ct);
                multiplier = mu?.Multiplier ?? 1m;
                baseMuId   = mu == null
                    ? cmd.MeasurementUnitId
                    : (await db.MeasurementUnits
                        .FirstOrDefaultAsync(m => m.Type == mu.Type && m.Multiplier == 1m, ct))?.Id
                      ?? cmd.MeasurementUnitId;
                baseAmount = cmd.Amount * multiplier;
            }

            var skuId     = Guid.NewGuid();
            var totalCost = cmd.Price * cmd.Amount;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var sku = new Sku
                    {
                        Id                = skuId,
                        SerialNumber      = serialNumber,
                        Price             = cmd.Price,
                        Amount            = cmd.Amount,
                        ExpirationDate    = cmd.ExpirationDate,
                        PurchaseDate      = DateTimeOffset.UtcNow,
                        ProductId         = cmd.ProductId,
                        ManufacturerId    = cmd.ManufacturerId,
                        MeasurementUnitId = cmd.MeasurementUnitId,
                        SupplierId        = cmd.SupplierId,
                        Status            = cmd.Status,
                    };

                    db.OrganizationSkus.Add(new OrganizationSku { OrganizationId = orgId, Sku = sku });

                    if (cmd.CashAccountId.HasValue)
                    {
                        await CashTransactionLinker.UpsertAsync(
                            db, balanceService,
                            CashTransactionSourceType.SkuPurchase, skuId,
                            orgId, cmd.CashAccountId.Value,
                            CashDirection.Out, CashTransactionType.SupplierPayment,
                            FinancePartnerType.Supplier, cmd.SupplierId,
                            totalCost, DateTimeOffset.UtcNow, null,
                            $"Sklad: {productName} ({serialNumber})",
                            userId, ct);
                    }

                    if (targetStockId.HasValue && cmd.Amount > 0 && baseMuId.HasValue)
                    {
                        db.StockSkus.Add(new StockSku
                        {
                            SkuId             = skuId,
                            StockId           = targetStockId.Value,
                            Amount            = baseAmount,
                            MeasurementUnitId = baseMuId.Value,
                        });
                        db.StockBalanceRegisters.Add(new StockBalanceRegister
                        {
                            StockId           = targetStockId.Value,
                            SkuId             = skuId,
                            ProductId         = cmd.ProductId,
                            MeasurementUnitId = baseMuId.Value,
                            PreviousAmount    = 0,
                            CurrentAmount     = baseAmount,
                            VariableAmount    = baseAmount,
                            Date              = DateTimeOffset.UtcNow,
                            Type              = (int)IncomeType.External,
                        });
                    }

                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException ex)
            {
                return (null, ex.Message);
            }

            return (skuId, null);
        }
    }
}
