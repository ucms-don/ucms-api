namespace Ucms.Application.Features.Manufacturers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteManufacturers
{
    public record Command(Guid[] Ids);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(int Deleted, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (db.Skus.Any(s => s.ManufacturerId != null && cmd.Ids.Contains(s.ManufacturerId.Value)))
                return (0, "Ishlab chiqaruvchilar SKUlarda ishlatilmoqda");
            var entities = await db.Manufacturers.AsTracking()
                .Where(f => cmd.Ids.Contains(f.Id)).ToListAsync(ct);
            foreach (var e in entities) e.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (entities.Count, null);
        }
    }
}
