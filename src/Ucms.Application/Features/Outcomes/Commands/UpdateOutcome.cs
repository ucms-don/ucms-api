namespace Ucms.Application.Features.Outcomes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateOutcome
{
    public record Command(Guid Id, string Name, string? Note, OutcomeType OutcomeType, OutcomeStatus OutcomeStatus,
        PaymentType PaymentType, DateTimeOffset OutcomeDate, Guid StockId, Guid? IncomeStockId,
        Guid? ExecutionId, IEnumerable<CreateOutcomeItemModel> OutcomeItems);

    public sealed class Handler(IUcmsDbContext db, IOutcomeService outcomeService,
        IWorkContext workContext, IOrganizationService organizationService)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            await outcomeService.ValidateOutcomeItems(cmd.OutcomeItems, cmd.StockId, ct);
            var outcome = await db.Outcomes.Include(i => i.OutcomeItems)
                .FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (outcome is null) return false;

            var strategy = db.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                try
                {
                    db.OutcomeItems.RemoveRange(outcome.OutcomeItems);
                    await db.SaveChangesAsync(ct);

                    outcome.Name = cmd.Name; outcome.Note = cmd.Note; outcome.OutcomeType = cmd.OutcomeType;
                    outcome.OutcomeStatus = cmd.OutcomeStatus; outcome.PaymentType = cmd.PaymentType;
                    outcome.OutcomeDate = cmd.OutcomeDate; outcome.StockId = cmd.StockId; outcome.ExecutionId = cmd.ExecutionId;
                    outcome.EmployeeId = workContext.EmployeeId;
                    outcome.EmployeeName = await organizationService.GetEmployeeName(workContext.EmployeeId);
                    outcome.OutcomeTransferStatus = (cmd.OutcomeType is OutcomeType.Broadcast or OutcomeType.Return) &&
                        cmd.IncomeStockId != null && cmd.OutcomeStatus == OutcomeStatus.Approved
                        ? OutcomeTransferStatus.Sent : null;
                    outcome.OutcomeItems = cmd.OutcomeItems.Select(s => new OutcomeItem
                    {
                        SkuId = s.SkuId, Amount = s.Amount, MeasurementUnitId = s.MeasurementUnitId
                    }).ToArray();
                    db.Outcomes.Update(outcome);
                    await db.SaveChangesAsync(ct);

                    if (outcome.OutcomeStatus == OutcomeStatus.Approved &&
                        (outcome.OutcomeType == OutcomeType.WriteOff || outcome.OutcomeType == OutcomeType.Usage))
                        await outcomeService.UpdateBalanceAsync(outcome, ct);

                    if ((outcome.OutcomeType is OutcomeType.Broadcast or OutcomeType.Return) && cmd.IncomeStockId != null)
                    {
                        var incomeOutcome = await db.IncomeOutcomes.FirstOrDefaultAsync(f => f.OutcomeId == outcome.Id, ct)
                            ?? outcomeService.CreateIncomeOutcome(outcome, cmd.IncomeStockId.Value);
                        if (outcome.OutcomeStatus == OutcomeStatus.Approved)
                        {
                            incomeOutcome.Income = outcomeService.CreateIncome(outcome, cmd.IncomeStockId.Value);
                            db.OrganizationSkus.AddRange(outcomeService.CreateOrganizationSkus(outcome, cmd.IncomeStockId.Value));
                        }
                        db.IncomeOutcomes.Update(incomeOutcome);
                        await db.SaveChangesAsync(ct);
                    }
                    await tx.CommitAsync(ct);
                }
                catch (Exception ex) { await tx.RollbackAsync(ct); throw new AppException(ex, "Xarajatni yangilashda xatolik"); }
            });
            return true;
        }
    }
}
