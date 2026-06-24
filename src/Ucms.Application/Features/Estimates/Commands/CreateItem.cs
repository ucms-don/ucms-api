namespace Ucms.Application.Features.Estimates.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateItem
{
    public record Command(
        Guid ProjectId, Guid EstimateId, Guid SectionId,
        Guid WorkTypeId, SurfaceType? SurfaceType, string? Description, Guid MeasurementUnitId, decimal Volume,
        decimal ClientUnitPrice, decimal BrigadeUnitPrice, decimal MaterialUnitPrice, int Order);

    public record Result(Guid Id, Guid WorkTypeId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null || (!ctx.IsOwner && ctx.OrganizationId != orgId))
                return (null, orgId is not null, null);

            var sectionExists = await db.EstimateSections
                .AnyAsync(s => s.Id == cmd.SectionId && s.EstimateId == cmd.EstimateId, ct);

            if (!sectionExists)
                return (null, false, "Bo'lim ushbu smetaga tegishli emas");

            var unitExists = await db.MeasurementUnits
                .AnyAsync(u => u.Id == cmd.MeasurementUnitId && !u.IsDeleted, ct);

            if (!unitExists)
                return (null, false, "O'lchov birligi topilmadi");

            var workTypeExists = await db.WorkTypes
                .AnyAsync(w => w.Id == cmd.WorkTypeId && !w.IsDeleted, ct);

            if (!workTypeExists)
                return (null, false, "Ish turi topilmadi");

            var item = new EstimateItem
            {
                Id                = Guid.NewGuid(),
                SectionId         = cmd.SectionId,
                WorkTypeId        = cmd.WorkTypeId,
                SurfaceType       = cmd.SurfaceType,
                Description       = cmd.Description,
                MeasurementUnitId = cmd.MeasurementUnitId,
                Volume            = cmd.Volume,
                ClientUnitPrice   = cmd.ClientUnitPrice,
                BrigadeUnitPrice  = cmd.BrigadeUnitPrice,
                MaterialUnitPrice = cmd.MaterialUnitPrice,
                Order             = cmd.Order,
            };

            await db.EstimateItems.AddAsync(item, ct);
            await db.SaveChangesAsync(ct);
            return (new Result(item.Id, item.WorkTypeId.Value), false, null);
        }
    }
}
