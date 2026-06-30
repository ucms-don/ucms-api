namespace Ucms.Application.Features.Suppliers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteSuppliers
{
    public record Command(Guid[] Ids);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(int Deleted, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (db.Skus.Any(s => s.SupplierId != null && cmd.Ids.Contains(s.SupplierId.Value)))
                return (0, "Ta'minotchilar SKUlarda ishlatilmoqda");
            var entities = await db.Suppliers.AsTracking()
                .Where(f => cmd.Ids.Contains(f.Id)).ToListAsync(ct);
            foreach (var e in entities) e.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (entities.Count, null);
        }
    }
}
