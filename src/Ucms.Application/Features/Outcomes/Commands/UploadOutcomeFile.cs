namespace Ucms.Application.Features.Outcomes.Commands;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions.Storage;
using Ucms.Application.Persistence;
using Ucms.Domain.Exceptions;

public static class UploadOutcomeFile
{
    public record Command(Guid OutcomeId, IFormFile File);

    public sealed class Handler(IUcmsDbContext db, IFileStorageClient storageClient)
    {
        public async Task<(FileEntryModel? Result, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (cmd.File.ContentType != "application/pdf")
                return (null, "Fayl PDF formatida bo'lishi kerak");

            var outcome = await db.Outcomes.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.OutcomeId, ct);
            if (outcome is null) return (null, "Topilmadi");

            var response = await storageClient.UploadAsync("outcomes", $"{cmd.OutcomeId}.pdf", cmd.File.OpenReadStream(), ct)
                ?? throw new AppException("Faylni yuklashda xatolik. / Ошибка при загрузке файла.");

            outcome.FilePath = response.FilePath;
            await db.SaveChangesAsync(ct);
            return (response, null);
        }
    }
}
