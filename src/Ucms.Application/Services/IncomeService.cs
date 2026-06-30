namespace Ucms.Application.Services;

using System.Threading;
using Microsoft.EntityFrameworkCore;
using Ucms.Domain.Entities;
using Ucms.Application.Persistence;

public class IncomeService(IUcmsDbContext dbContext) : IIncomeService
{
    public async Task UpdateBalanceAsync(Income income, CancellationToken cancellationToken)
    {
        var skuIds = income.IncomeItems.Select(s => s.SkuId);
        var measurementUnits = await dbContext.MeasurementUnits.ToListAsync(cancellationToken);
        var skuProductIds = await GetSkuProductIds(skuIds, cancellationToken);
        var stockBalanceRegistry = new List<StockBalanceRegister>();

        // update existing skus
        var existingStockSkus = await CalcExistingSkuAmountAsync(income, skuIds, measurementUnits, skuProductIds, stockBalanceRegistry, cancellationToken);
        dbContext.StockSkus.UpdateRange(existingStockSkus);

        // add new skus
        var newStockSkus = CalcNewSkuAmountsAsync(income, measurementUnits, skuProductIds, stockBalanceRegistry, existingStockSkus);
        dbContext.StockSkus.AddRange(newStockSkus);

        dbContext.StockBalanceRegisters.AddRange(stockBalanceRegistry);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<StockSku>> CalcExistingSkuAmountAsync(Income income, IEnumerable<Guid> skuIds, List<MeasurementUnit> measurementUnits, Dictionary<Guid, Guid> skuProductIds, List<StockBalanceRegister> stockBalanceRegistry, CancellationToken cancellationToken)
    {
        var existingStockSkus = await dbContext.StockSkus
            .Where(w => w.StockId == income.StockId && skuIds.Contains(w.SkuId))
            .ToListAsync(cancellationToken);

        foreach (var existingStockSku in existingStockSkus)
        {
            var incomeItem = income.IncomeItems.First(f => f.SkuId == existingStockSku.SkuId);
            var measurementUnit = measurementUnits.First(f => f.Id == incomeItem.MeasurementUnitId);
            var basicMU = measurementUnits.FirstOrDefault(f => f.Type == measurementUnit!.Type && f.Multiplier == 1);
            var measurementUnitId = basicMU != null ? basicMU.Id : measurementUnit.Id;
            var skuProductId = skuProductIds.GetValueOrDefault(existingStockSku.SkuId);
            var amount = incomeItem.Amount * (measurementUnit?.Multiplier ?? 0);

            stockBalanceRegistry.Add(new StockBalanceRegister
            {
                StockId = existingStockSku.StockId,
                SkuId = existingStockSku.SkuId,
                ProductId = skuProductId,
                MeasurementUnitId = measurementUnitId,
                PreviousAmount = existingStockSku.Amount,
                CurrentAmount = existingStockSku.Amount + amount,
                VariableAmount = amount,
                Date = income.IncomeDate,
                Type = (int)income.IncomeType
            });

            existingStockSku.Amount += amount;
            existingStockSku.MeasurementUnitId = measurementUnitId;
        }

        return existingStockSkus;
    }

    private static List<StockSku> CalcNewSkuAmountsAsync(Income income, List<MeasurementUnit> measurementUnits, Dictionary<Guid, Guid> skuProductIds, List<StockBalanceRegister> stockBalanceRegistry, List<StockSku> existingStockSkus)
    {
        var newIncomeItems = income.IncomeItems.Where(w => !existingStockSkus.Any(a => a.SkuId == w.SkuId));
        var newStockSkus = new List<StockSku>();
        foreach (var newIncomeItem in newIncomeItems)
        {
            var measurementUnit = measurementUnits.First(f => f.Id == newIncomeItem.MeasurementUnitId);
            var basicMU = measurementUnits.FirstOrDefault(w => w.Type == measurementUnit!.Type && w.Multiplier == 1);
            var amount = newIncomeItem.Amount * measurementUnit.Multiplier;
            var skuProductId = skuProductIds.GetValueOrDefault(newIncomeItem.SkuId);

            newStockSkus.Add(new StockSku
            {
                SkuId = newIncomeItem.SkuId,
                StockId = income.StockId,
                Amount = amount,
                MeasurementUnitId = basicMU?.Id,
            });
            stockBalanceRegistry.Add(new StockBalanceRegister
            {
                StockId = income.StockId,
                SkuId = newIncomeItem.SkuId,
                ProductId = skuProductId,
                MeasurementUnitId = basicMU?.Id ?? Guid.Empty,
                PreviousAmount = 0,
                CurrentAmount = amount,
                VariableAmount = amount,
                Date = income.IncomeDate,
                Type = (int)income.IncomeType
            });
        }

        return newStockSkus;
    }

    private Task<Dictionary<Guid, Guid>> GetSkuProductIds(IEnumerable<Guid> skuIds, CancellationToken cancellationToken)
    {
        return dbContext.Skus
            .Where(w => skuIds.Contains(w.Id))
            .ToDictionaryAsync(a => a.Id, a => a.ProductId, cancellationToken);
    }
}
