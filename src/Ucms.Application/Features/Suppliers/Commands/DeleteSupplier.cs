namespace Ucms.Application.Features.Suppliers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteSupplier
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var entity = await db.Suppliers.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (entity is null) return (true, null);
            if (db.Skus.Any(s => s.SupplierId == cmd.Id))
                return (false, "Ta'minotchi SKUlarda ishlatilmoqda");
            entity.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
