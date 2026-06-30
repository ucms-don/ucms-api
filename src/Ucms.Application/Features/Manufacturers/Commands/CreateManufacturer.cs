namespace Ucms.Application.Features.Manufacturers.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateManufacturer
{
    public record Command(string Name, string NameRu, string? NameEn, string? NameKa, string? Code);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (cmd.Code is not null && await db.Manufacturers.AnyAsync(f => f.Code == cmd.Code, ct))
                return (null, $"'{cmd.Code}' kodi allaqachon mavjud");

            var entity = new Manufacturer
            {
                Name = cmd.Name, NameRu = cmd.NameRu, NameEn = cmd.NameEn,
                NameKa = cmd.NameKa, Code = cmd.Code
            };
            db.Manufacturers.Add(entity);
            await db.SaveChangesAsync(ct);
            return (entity.Id, null);
        }
    }
}
