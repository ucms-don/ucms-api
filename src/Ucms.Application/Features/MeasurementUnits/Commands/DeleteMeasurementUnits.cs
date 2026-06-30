namespace Ucms.Application.Features.MeasurementUnits.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteMeasurementUnits
{
    public record Command(Guid[] Ids);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var entities = await db.MeasurementUnits.AsTracking()
                .Where(f => cmd.Ids.Contains(f.Id)).ToListAsync(ct);
            if (entities.Count == 0) return false;
            foreach (var e in entities) e.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
