namespace Ucms.Application.Features.Incomes.Commands;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Incomes.DTOs;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateIncome
{
    public record Command(Guid Id, string Name, string? Note, IncomeType IncomeType, IncomeStatus IncomeStatus,
        PaymentType PaymentType, DateTimeOffset IncomeDate, Guid StockId, IEnumerable<CreateIncomeItemModel> IncomeItems);

    public sealed class Handler(IUcmsDbContext db, IIncomeService incomeService, IOutcomeService outcomeService,
        IWorkContext workContext, IOrganizationService organizationService, ILogger<Handler> logger)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var income = await db.Incomes.Include(i => i.IncomeItems)
                .FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (income is null) return false;

            var strategy = db.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                try
                {
                    db.IncomeItems.RemoveRange(income.IncomeItems);
                    await db.SaveChangesAsync(ct);

                    income.Name = cmd.Name; income.Note = cmd.Note; income.IncomeType = cmd.IncomeType;
                    income.IncomeStatus = cmd.IncomeStatus; income.PaymentType = cmd.PaymentType;
                    income.IncomeDate = cmd.IncomeDate; income.StockId = cmd.StockId;
                    income.EmployeeId = workContext.EmployeeId;
                    income.EmployeeName = await organizationService.GetEmployeeName(workContext.EmployeeId);
                    income.IncomeItems = cmd.IncomeItems.Select(s => new IncomeItem
                    {
                        Amount = s.Amount, SkuId = s.SkuId, MeasurementUnitId = s.MeasurementUnitId
                    }).ToArray();

                    db.Incomes.Update(income);
                    await db.SaveChangesAsync(ct);

                    if (income.IncomeTransferStatus == IncomeTransferStatus.Received)
                    {
                        income.IncomeTransferStatus = income.IncomeStatus switch
                        {
                            IncomeStatus.Approved  => IncomeTransferStatus.Approved,
                            IncomeStatus.Cancelled => IncomeTransferStatus.Cancelled,
                            _                      => income.IncomeTransferStatus
                        };
                    }
                    if (income.IncomeStatus == IncomeStatus.Approved)
                    {
                        await incomeService.UpdateBalanceAsync(income, ct);
                        await outcomeService.UpdateIncomeOutcome(income.Id, ct);
                    }
                    await tx.CommitAsync(ct);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(ct);
                    logger.LogError("Error updating income: {Message}", ex.Message);
                    throw new AppException(ex.Message);
                }
            });
            return true;
        }
    }
}
