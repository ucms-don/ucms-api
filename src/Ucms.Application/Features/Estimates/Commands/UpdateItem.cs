namespace Ucms.Application.Features.Estimates.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class UpdateItem
{
    public record Command(
        Guid ProjectId, Guid EstimateId, Guid ItemId,
        Guid WorkTypeId, string? Description, Guid MeasurementUnitId, decimal Volume,
        decimal ClientUnitPrice, decimal BrigadeUnitPrice, decimal MaterialUnitPrice, int Order);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null || (!ctx.IsOwner && ctx.OrganizationId != orgId))
                return (false, orgId is not null);

            var item = await db.EstimateItems
                .Include(i => i.Section)
                .FirstOrDefaultAsync(i => i.Id == cmd.ItemId && i.Section!.EstimateId == cmd.EstimateId, ct);

            if (item is null) return (true, false);

            item.WorkTypeId        = cmd.WorkTypeId;
            item.Description       = cmd.Description;
            item.MeasurementUnitId = cmd.MeasurementUnitId;
            item.Volume            = cmd.Volume;
            item.ClientUnitPrice   = cmd.ClientUnitPrice;
            item.BrigadeUnitPrice  = cmd.BrigadeUnitPrice;
            item.MaterialUnitPrice = cmd.MaterialUnitPrice;
            item.Order             = cmd.Order;

            db.EstimateItems.Update(item);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
