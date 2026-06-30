namespace Ucms.Application.Features.Outcomes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateOutcomeStatus
{
    public record Command(Guid Id, OutcomeStatus Status);

    public sealed class Handler(IUcmsDbContext db, IOutcomeService outcomeService)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var outcome = await db.Outcomes.Include(i => i.OutcomeItems)
                .FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (outcome is null) return (true, null);
            if (outcome.OutcomeStatus == OutcomeStatus.Approved)
                return (false, "Xarajat allaqachon tasdiqlangan");

            await outcomeService.ValidateOutcomeItems(outcome.OutcomeItems, outcome.StockId, ct);
            var strategy = db.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                try
                {
                    outcome.OutcomeStatus = cmd.Status;
                    if (outcome.OutcomeType == OutcomeType.Broadcast && cmd.Status == OutcomeStatus.Approved)
                        outcome.OutcomeTransferStatus = OutcomeTransferStatus.Sent;
                    db.Outcomes.Update(outcome);
                    await db.SaveChangesAsync(ct);

                    if (cmd.Status == OutcomeStatus.Approved && (outcome.OutcomeType is OutcomeType.WriteOff or OutcomeType.Usage))
                        await outcomeService.UpdateBalanceAsync(outcome, ct);

                    if (cmd.Status == OutcomeStatus.Approved && (outcome.OutcomeType is OutcomeType.Broadcast or OutcomeType.Return))
                    {
                        var io = await db.IncomeOutcomes.FirstOrDefaultAsync(f => f.OutcomeId == outcome.Id, ct);
                        if (io != null)
                        {
                            io.Income = outcomeService.CreateIncome(outcome, io.IncomeStockId);
                            db.OrganizationSkus.AddRange(outcomeService.CreateOrganizationSkus(outcome, io.IncomeStockId));
                            db.IncomeOutcomes.Update(io);
                            await db.SaveChangesAsync(ct);
                        }
                    }
                    await tx.CommitAsync(ct);
                }
                catch (Exception ex) { await tx.RollbackAsync(ct); throw new AppException(ex, "Xarajat holatini o'zgartirishda xatolik"); }
            });
            return (false, null);
        }
    }
}
