namespace Ucms.Application.Features.WorkTypes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;

public static class CreateWorkType
{
    public record Command(string Name, string NameRu, string? NameEn, string? NameKa,
        Guid? MeasurementUnitId, string? Code);

    public sealed class Handler(IUcmsDbContext db, IWorkTypeCodeGenerator codeGenerator)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.WorkTypes.AnyAsync(f => f.Name == cmd.Name, ct))
                return (null, $"'{cmd.Name}' nomli ish turi allaqachon mavjud");

            // Code qo'lda kiritilmagan bo'lsa, avtomatik generatsiya qilinadi.
            var code = string.IsNullOrWhiteSpace(cmd.Code)
                ? await codeGenerator.GenerateAsync(ct)
                : cmd.Code.Trim();

            if (await db.WorkTypes.IgnoreQueryFilters().AnyAsync(f => f.Code == code, ct))
                return (null, $"'{code}' kodli ish turi allaqachon mavjud");

            var entity = new WorkType
            {
                Name = cmd.Name, NameRu = cmd.NameRu, NameEn = cmd.NameEn, NameKa = cmd.NameKa,
                MeasurementUnitId = cmd.MeasurementUnitId, Code = code,
            };
            db.WorkTypes.Add(entity);
            await db.SaveChangesAsync(ct);
            return (entity.Id, null);
        }
    }
}
