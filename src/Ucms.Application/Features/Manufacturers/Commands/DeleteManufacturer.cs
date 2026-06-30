namespace Ucms.Application.Features.Manufacturers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteManufacturer
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var entity = await db.Manufacturers.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (entity is null) return (true, null);
            if (db.Skus.Any(s => s.ManufacturerId == cmd.Id))
                return (false, "Ishlab chiqaruvchi SKUlarda ishlatilmoqda");
            entity.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
