namespace Ucms.Application.Features.MeasurementUnits.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateMeasurementUnit
{
    public record Command(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, MeasurementUnitType Type, decimal Multiplier);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(cmd.Code) &&
                await db.MeasurementUnits.AnyAsync(a => a.Id != cmd.Id && a.Code == cmd.Code, ct))
                return (false, $"Kod '{cmd.Code}' allaqachon mavjud");
            var entity = await db.MeasurementUnits.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (entity is null) return (true, null);
            entity.Type = cmd.Type; entity.Multiplier = cmd.Multiplier;
            entity.Name = cmd.Name; entity.NameRu = cmd.NameRu; entity.NameEn = cmd.NameEn; entity.NameKa = cmd.NameKa;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
