namespace Ucms.Application.Features.WorkTypes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateWorkType
{
    public record Command(string Name, string NameRu, string? NameEn, string? NameKa);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.WorkTypes.AnyAsync(f => f.Name == cmd.Name, ct))
                return (null, $"'{cmd.Name}' nomli ish turi allaqachon mavjud");

            var entity = new WorkType
            {
                Name = cmd.Name, NameRu = cmd.NameRu, NameEn = cmd.NameEn, NameKa = cmd.NameKa,
            };
            db.WorkTypes.Add(entity);
            await db.SaveChangesAsync(ct);
            return (entity.Id, null);
        }
    }
}
