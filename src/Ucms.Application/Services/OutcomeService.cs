namespace Ucms.Application.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;
using Ucms.Domain.Entities;
using Ucms.Application.Persistence;
using Ucms.Application.Features.Outcomes.DTOs;

public class OutcomeService(IUcmsDbContext dbContext, IMapper mapper) : IOutcomeService
{
    public async Task UpdateBalanceAsync(Outcome outcome, CancellationToken cancellationToken)
    {
        var stockSkus = await GetStockSkus(outcome);
        var skuProductIds = await GetSkuProductIds(outcome);
        var measurementUnits = await GetMeasurementUnits(outcome.OutcomeItems);
        var stockBalanceRegistry = new List<StockBalanceRegister>();

        foreach (var outcomeItem in outcome.OutcomeItems)
        {
            var measurementUnit = measurementUnits.FirstOrDefault(f => f.Id == outcomeItem.MeasurementUnitId);
            if (measurementUnit == null)
                continue;

            var stockSku = stockSkus.OrderByDescending(o => o.Amount).FirstOrDefault(f => f.SkuId == outcomeItem.SkuId);
            if (stockSku == null)
                continue;

            var amount = outcomeItem.Amount * measurementUnit.Multiplier;
            if (amount > stockSku.Amount)
                throw new AppException($"Недостаточное количество продукта: {outcomeItem.SkuId}");

            stockBalanceRegistry.Add(new StockBalanceRegister
            {
                StockId = stockSku.StockId,
                SkuId = stockSku.SkuId,
                ProductId = skuProductIds.GetValueOrDefault(outcomeItem.SkuId),
                MeasurementUnitId = stockSku.MeasurementUnitId ?? Guid.Empty,
                PreviousAmount = stockSku.Amount,
                CurrentAmount = stockSku.Amount - amount,
                VariableAmount = -amount,
                Date = outcome.OutcomeDate,
                Type = (int)outcome.OutcomeType
            });
            stockSku.Amount -= amount;
        }

        dbContext.StockSkus.UpdateRange(stockSkus);
        dbContext.StockBalanceRegisters.AddRange(stockBalanceRegistry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateIncomeOutcome(Guid incomeId, CancellationToken cancellationToken)
    {
        var outcome = await dbContext.Outcomes
            .Include(i => i.OutcomeItems)
            .FirstOrDefaultAsync(f => f.IncomeOutcome!.IncomeId == incomeId, cancellationToken);

        if (outcome == null)
            return;

        outcome.OutcomeTransferStatus = OutcomeTransferStatus.Approved;
        dbContext.Outcomes.Update(outcome);
        await dbContext.SaveChangesAsync(cancellationToken);

        await UpdateDemandStatus(outcome.Id, StockDemandBroadcastStatus.Approved, cancellationToken);
        await UpdateBalanceAsync(outcome, cancellationToken);
    }

    public async Task CancelIncomeOutcome(Guid incomeId, CancellationToken cancellationToken)
    {
        var outcome = await dbContext.Outcomes.FirstOrDefaultAsync(w => w.IncomeOutcome!.IncomeId == incomeId, cancellationToken);
        if (outcome == null)
            return;

        outcome.OutcomeTransferStatus = OutcomeTransferStatus.Cancelled;
        dbContext.Outcomes.Update(outcome);
        await dbContext.SaveChangesAsync(cancellationToken);

        await UpdateDemandStatus(outcome.Id, StockDemandBroadcastStatus.Cancelled, cancellationToken);
    }

    public IncomeOutcome CreateIncomeOutcome(Outcome outcome, Guid incomeStockId)
    {
        return new IncomeOutcome
        {
            OutcomeId = outcome.Id,
            IncomeStockId = incomeStockId,
            OutcomeStockId = outcome.StockId,
            Date = outcome.OutcomeDate
        };
    }

    public Income CreateIncome(Outcome outcome, Guid incomeStockId)
    {
        return new Income
        {
            Name = outcome.Name,
            Note = outcome.Note,
            IncomeDate = outcome.OutcomeDate,
            PaymentType = outcome.PaymentType,
            IncomeStatus = IncomeStatus.Draft,
            IncomeTransferStatus = IncomeTransferStatus.Received,
            IncomeType = outcome.OutcomeType == OutcomeType.Broadcast ? IncomeType.Internal : IncomeType.Return,
            StockId = incomeStockId,
            IncomeItems = [.. outcome.OutcomeItems.Select(s => new IncomeItem
            {
                SkuId = s.SkuId,
                Amount = s.Amount,
                MeasurementUnitId = s.MeasurementUnitId
            })]
        };
    }

    public IEnumerable<OrganizationSku> CreateOrganizationSkus(Outcome outcome, Guid incomeStockId)
    {
        var incomeStock = dbContext.Stocks.FirstOrDefault(f => f.Id == incomeStockId);
        if (incomeStock == null)
            return [];

        var skuIds = outcome.OutcomeItems.Select(s => s.SkuId).ToArray();
        var existingSkus = dbContext.OrganizationSkus
            .Where(w => w.OrganizationId == incomeStock.OrganizationId && skuIds.Contains(w.SkuId))
            .ToList();
        return outcome.OutcomeItems
            .Where(w => !existingSkus.Any(a => a.SkuId == w.SkuId))
            .Select(s => new OrganizationSku
            {
                SkuId = s.SkuId,
                OrganizationId = incomeStock.OrganizationId
            });
    }

    public async Task ValidateOutcomeItems(IEnumerable<CreateOutcomeItemModel> outcomeItems, Guid stockId, CancellationToken cancellationToken)
    {
        var measurementUnitIds = outcomeItems.Select(s => s.MeasurementUnitId).ToList();
        var measurementUnits = await dbContext.MeasurementUnits.Where(w => measurementUnitIds.Contains(w.Id)).ToListAsync(cancellationToken);

        var insufficientSkuIds = new List<Guid>();
        foreach (var item in outcomeItems)
        {
            var measurementUnit = measurementUnits.FirstOrDefault(f => f.Id == item.MeasurementUnitId);
            if (measurementUnit == null)
                continue;

            var hasEnough = await dbContext.StockSkus
                .AnyAsync(a => a.StockId == stockId
                            && a.SkuId == item.SkuId
                            && a.Amount >= item.Amount * measurementUnit.Multiplier, cancellationToken);
            if (!hasEnough)
                insufficientSkuIds.Add(item.SkuId);
        }

        if (insufficientSkuIds.Count > 0)
        {
            var numbers = await dbContext.Skus
                .Where(s => insufficientSkuIds.Contains(s.Id))
                .Select(s => s.SerialNumber)
                .ToListAsync(cancellationToken);

            var list = string.Join(", ", numbers);
            throw new AppException(string.IsNullOrWhiteSpace(list)
                ? "На выбранном складе недостаточно количества товара"
                : $"На выбранном складе недостаточно количества товара: {list}");
        }
    }

    public async Task ValidateOutcomeItems(IEnumerable<OutcomeItem> outcomeItems, Guid stockId, CancellationToken cancellationToken)
    {
        var outcomeItemsModel = mapper.Map<List<CreateOutcomeItemModel>>(outcomeItems.ToList());
        await ValidateOutcomeItems(outcomeItemsModel, stockId, cancellationToken);
    }

    private Task<List<StockSku>> GetStockSkus(Outcome outcome)
    {
        var skuIds = outcome.OutcomeItems.Select(s => s.SkuId);
        return dbContext.StockSkus
            .Where(w => w.StockId == outcome.StockId && skuIds.Contains(w.SkuId))
            .ToListAsync();
    }

    private Task<Dictionary<Guid, Guid>> GetSkuProductIds(Outcome outcome)
    {
        var skuIds = outcome.OutcomeItems.Select(s => s.SkuId);
        return dbContext.Skus
            .Where(w => skuIds.Contains(w.Id))
            .ToDictionaryAsync(a => a.Id, a => a.ProductId);
    }

    private async Task<List<MeasurementUnit>> GetMeasurementUnits(ICollection<OutcomeItem> outcomeItem)
    {
        var measurementUnitIds = outcomeItem.Select(s => s.MeasurementUnitId);
        return await dbContext.MeasurementUnits.Where(w => measurementUnitIds.Contains(w.Id)).ToListAsync();
    }

    private async Task UpdateDemandStatus(Guid outcomeId, StockDemandBroadcastStatus status, CancellationToken cancellationToken)
    {
        var demand = await dbContext.StockDemands.FirstOrDefaultAsync(f => f.OutcomeId == outcomeId, cancellationToken);
        if (demand == null)
            return;

        demand.BroadcastStatus = status;
        dbContext.StockDemands.Update(demand);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
