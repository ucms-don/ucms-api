namespace Ucms.Application.Features.WorkTypes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Application.Services;

public static class UpdateWorkType
{
    public record Command(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        Guid? MeasurementUnitId, string? Code);

    public sealed class Handler(IUcmsDbContext db, IWorkTypeCodeGenerator codeGenerator)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.WorkTypes.AnyAsync(a => a.Id != cmd.Id && a.Name == cmd.Name, ct))
                return (false, $"'{cmd.Name}' nomli ish turi allaqachon mavjud");

            var entity = await db.WorkTypes.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (entity is null) return (true, null);

            // Code qo'lda kiritilmagan bo'lsa, avtomatik generatsiya qilinadi (mavjud bo'lsa o'zgarmaydi).
            var code = string.IsNullOrWhiteSpace(cmd.Code)
                ? (entity.Code ?? await codeGenerator.GenerateAsync(ct))
                : cmd.Code.Trim();

            if (await db.WorkTypes.IgnoreQueryFilters().AnyAsync(f => f.Id != cmd.Id && f.Code == code, ct))
                return (false, $"'{code}' kodli ish turi allaqachon mavjud");

            entity.Name = cmd.Name; entity.NameRu = cmd.NameRu; entity.NameEn = cmd.NameEn; entity.NameKa = cmd.NameKa;
            entity.MeasurementUnitId = cmd.MeasurementUnitId; entity.Code = code;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
