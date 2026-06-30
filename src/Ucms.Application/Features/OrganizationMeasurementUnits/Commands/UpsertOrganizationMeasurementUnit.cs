namespace Ucms.Application.Features.OrganizationMeasurementUnits.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class UpsertOrganizationMeasurementUnit
{
    public record Command(MeasurementUnitType Type, Guid MeasurementUnitId);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext)
    {
        public async Task<Guid> HandleAsync(Command cmd, CancellationToken ct)
        {
            var entity = await db.OrganizationMeasurementUnits.AsTracking()
                .FirstOrDefaultAsync(f => f.OrganizationId == workContext.TenantId && f.Type == cmd.Type, ct);
            if (entity is null)
            {
                entity = new OrganizationMeasurementUnit
                {
                    Type = cmd.Type, OrganizationId = workContext.TenantId ?? Guid.Empty,
                    MeasurementUnitId = cmd.MeasurementUnitId
                };
                db.OrganizationMeasurementUnits.Add(entity);
            }
            else entity.MeasurementUnitId = cmd.MeasurementUnitId;
            await db.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
