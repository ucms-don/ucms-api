namespace Ucms.Application.Features.WorkTypes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class UpdateWorkType
{
    public record Command(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        Guid? MeasurementUnitId);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.WorkTypes.AnyAsync(a => a.Id != cmd.Id && a.Name == cmd.Name, ct))
                return (false, $"'{cmd.Name}' nomli ish turi allaqachon mavjud");

            var entity = await db.WorkTypes.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (entity is null) return (true, null);

            entity.Name = cmd.Name; entity.NameRu = cmd.NameRu; entity.NameEn = cmd.NameEn; entity.NameKa = cmd.NameKa;
            entity.MeasurementUnitId = cmd.MeasurementUnitId;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
