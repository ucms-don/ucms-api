namespace Ucms.Application.Features.StockDemands.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateStockDemandStatus
{
    public record Command(Guid Id, StockDemandStatus Status);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var demand = await db.StockDemands.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (demand is null) return (true, null);
            if (demand.DemandStatus is StockDemandStatus.Approved or StockDemandStatus.Cancelled)
                return (false, "Talab holati o'zgartirilmaydi");
            demand.DemandStatus = cmd.Status;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
