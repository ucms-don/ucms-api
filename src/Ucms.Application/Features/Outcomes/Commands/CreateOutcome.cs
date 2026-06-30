namespace Ucms.Application.Features.Outcomes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class CreateOutcome
{
    public record Command(string Name, string? Note, OutcomeType OutcomeType, OutcomeStatus OutcomeStatus,
        PaymentType PaymentType, DateTimeOffset OutcomeDate, Guid StockId, Guid? IncomeStockId,
        Guid? ExecutionId, IEnumerable<CreateOutcomeItemModel> OutcomeItems);

    public sealed class Handler(IUcmsDbContext db, IOutcomeService outcomeService,
        IWorkContext workContext, IOrganizationService organizationService)
    {
        public async Task<Guid> HandleAsync(Command cmd, CancellationToken ct)
        {
            await outcomeService.ValidateOutcomeItems(cmd.OutcomeItems, cmd.StockId, ct);
            var strategy = db.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                try
                {
                    var outcome = new Outcome
                    {
                        Name = cmd.Name, Note = cmd.Note, OutcomeType = cmd.OutcomeType,
                        OutcomeStatus = cmd.OutcomeStatus, PaymentType = cmd.PaymentType,
                        OutcomeDate = cmd.OutcomeDate, StockId = cmd.StockId, ExecutionId = cmd.ExecutionId,
                        EmployeeId = workContext.EmployeeId,
                        EmployeeName = await organizationService.GetEmployeeName(workContext.EmployeeId),
                        OutcomeItems = [.. cmd.OutcomeItems.Select(s => new OutcomeItem
                        {
                            SkuId = s.SkuId, Amount = s.Amount, MeasurementUnitId = s.MeasurementUnitId
                        })]
                    };
                    db.Outcomes.Add(outcome);
                    await db.SaveChangesAsync(ct);

                    if (outcome.OutcomeStatus == OutcomeStatus.Approved &&
                        (outcome.OutcomeType == OutcomeType.WriteOff || outcome.OutcomeType == OutcomeType.Usage ||
                        (outcome.OutcomeType == OutcomeType.Return && cmd.IncomeStockId == null)))
                        await outcomeService.UpdateBalanceAsync(outcome, ct);

                    if (outcome.OutcomeType is OutcomeType.Broadcast or OutcomeType.Return && cmd.IncomeStockId != null)
                    {
                        var incomeOutcome = outcomeService.CreateIncomeOutcome(outcome, cmd.IncomeStockId.Value);
                        if (outcome.OutcomeStatus == OutcomeStatus.Approved)
                        {
                            incomeOutcome.Income = outcomeService.CreateIncome(outcome, cmd.IncomeStockId.Value);
                            db.OrganizationSkus.AddRange(outcomeService.CreateOrganizationSkus(outcome, cmd.IncomeStockId.Value));
                        }
                        db.IncomeOutcomes.Add(incomeOutcome);
                        await db.SaveChangesAsync(ct);
                    }
                    await tx.CommitAsync(ct);
                    return outcome.Id;
                }
                catch (Exception ex) { await tx.RollbackAsync(ct); throw new AppException(ex, "Xarajat yaratishda xatolik"); }
            });
        }
    }
}
