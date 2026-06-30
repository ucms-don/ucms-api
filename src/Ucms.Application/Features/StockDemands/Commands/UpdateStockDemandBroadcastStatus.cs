namespace Ucms.Application.Features.StockDemands.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateStockDemandBroadcastStatus
{
    public record Command(Guid Id, Guid OutcomeId, StockDemandBroadcastStatus Status);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var demand = await db.StockDemands.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (demand is null) return (true, null);
            if (demand.BroadcastStatus is StockDemandBroadcastStatus.Approved or StockDemandBroadcastStatus.Cancelled)
                return (false, "Broadcast holati o'zgartirilmaydi");
            demand.BroadcastStatus = cmd.Status;
            demand.OutcomeId = cmd.OutcomeId;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
