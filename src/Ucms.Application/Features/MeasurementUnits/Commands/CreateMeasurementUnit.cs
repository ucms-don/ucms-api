namespace Ucms.Application.Features.MeasurementUnits.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateMeasurementUnit
{
    public record Command(string Code, string Name, string NameRu, string? NameEn, string? NameKa,
        MeasurementUnitType Type, decimal Multiplier);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.MeasurementUnits.AnyAsync(f => f.Code == cmd.Code, ct))
                return (null, $"Kod '{cmd.Code}' allaqachon mavjud");
            var entity = new MeasurementUnit
            {
                Code = cmd.Code, Multiplier = cmd.Multiplier, Type = cmd.Type,
                Name = cmd.Name, NameRu = cmd.NameRu, NameEn = cmd.NameEn, NameKa = cmd.NameKa
            };
            db.MeasurementUnits.Add(entity);
            await db.SaveChangesAsync(ct);
            return (entity.Id, null);
        }
    }
}
