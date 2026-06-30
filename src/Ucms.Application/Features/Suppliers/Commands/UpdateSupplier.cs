namespace Ucms.Application.Features.Suppliers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class UpdateSupplier
{
    public record Command(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa, string Code);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var entity = await db.Suppliers.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (entity is null) return false;
            entity.Name = cmd.Name; entity.NameRu = cmd.NameRu;
            entity.NameEn = cmd.NameEn; entity.NameKa = cmd.NameKa; entity.Code = cmd.Code;
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
