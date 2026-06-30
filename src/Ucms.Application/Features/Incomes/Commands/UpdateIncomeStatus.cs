namespace Ucms.Application.Features.Incomes.Commands;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateIncomeStatus
{
    public record Command(Guid Id, IncomeStatus Status);

    public sealed class Handler(IUcmsDbContext db, IIncomeService incomeService, IOutcomeService outcomeService, ILogger<Handler> logger)
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
                    income.IncomeStatus = cmd.Status;
                    if (income.IncomeTransferStatus == IncomeTransferStatus.Received)
                    {
                        income.IncomeTransferStatus = cmd.Status switch
                        {
                            IncomeStatus.Approved  => IncomeTransferStatus.Approved,
                            IncomeStatus.Cancelled => IncomeTransferStatus.Cancelled,
                            _                      => income.IncomeTransferStatus
                        };
                    }
                    db.Incomes.Update(income);
                    await db.SaveChangesAsync(ct);

                    if (income.IncomeStatus == IncomeStatus.Approved)
                    {
                        await incomeService.UpdateBalanceAsync(income, ct);
                        await outcomeService.UpdateIncomeOutcome(income.Id, ct);
                    }
                    else if (income.IncomeStatus == IncomeStatus.Cancelled)
                        await outcomeService.CancelIncomeOutcome(income.Id, ct);

                    await tx.CommitAsync(ct);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(ct);
                    logger.LogError("Error updating income status: {Message}", ex.Message);
                    throw new AppException(ex.Message);
                }
            });
            return true;
        }
    }
}
