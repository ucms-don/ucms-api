namespace Ucms.Application.Features.Incomes.Commands;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Incomes.DTOs;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateIncome
{
    public record Command(string Name, string? Note, IncomeType IncomeType, IncomeStatus IncomeStatus,
        PaymentType PaymentType, DateTimeOffset IncomeDate, Guid StockId, IEnumerable<CreateIncomeItemModel> IncomeItems);

    public sealed class Handler(IUcmsDbContext db, IIncomeService incomeService,
        IWorkContext workContext, IOrganizationService organizationService, ILogger<Handler> logger)
    {
        public async Task<Guid> HandleAsync(Command cmd, CancellationToken ct)
        {
            var strategy = db.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                try
                {
                    var income = new Income
                    {
                        Name = cmd.Name,
                        Note = cmd.Note,
                        IncomeType = cmd.IncomeType,
                        IncomeStatus = cmd.IncomeStatus,
                        PaymentType = cmd.PaymentType,
                        IncomeDate = cmd.IncomeDate,
                        StockId = cmd.StockId,
                        EmployeeId = workContext.EmployeeId,
                        EmployeeName = await organizationService.GetEmployeeName(workContext.EmployeeId),
                        IncomeItems = [.. cmd.IncomeItems.Select(s => new IncomeItem
                        {
                            Amount = s.Amount,
                            SkuId = s.SkuId,
                            MeasurementUnitId = s.MeasurementUnitId
                        })]
                    };
                    db.Incomes.Add(income);
                    await db.SaveChangesAsync(ct);
                    if (income.IncomeStatus == IncomeStatus.Approved)
                        await incomeService.UpdateBalanceAsync(income, ct);
                    await tx.CommitAsync(ct);
                    return income.Id;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while creating income");
                }
                await tx.RollbackAsync(ct);
                return Guid.Empty;
            });
        }
    }
}
