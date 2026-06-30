namespace Ucms.Application.Features.Suppliers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateSupplier
{
    public record Command(string Name, string NameRu, string? NameEn, string? NameKa, string Code);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.Suppliers.AnyAsync(f => f.Code == cmd.Code, ct))
                return (null, $"'{cmd.Code}' kodi allaqachon mavjud");

            var entity = new Supplier
            {
                Name = cmd.Name, NameRu = cmd.NameRu, NameEn = cmd.NameEn,
                NameKa = cmd.NameKa, Code = cmd.Code
            };
            db.Suppliers.Add(entity);
            await db.SaveChangesAsync(ct);
            return (entity.Id, null);
        }
    }
}
