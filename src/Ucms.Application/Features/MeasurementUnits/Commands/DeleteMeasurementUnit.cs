namespace Ucms.Application.Features.MeasurementUnits.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteMeasurementUnit
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var entity = await db.MeasurementUnits.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
            if (entity is null) return false;
            entity.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
