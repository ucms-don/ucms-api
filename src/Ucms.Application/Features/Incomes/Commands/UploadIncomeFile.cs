namespace Ucms.Application.Features.Incomes.Commands;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions.Storage;
using Ucms.Application.Persistence;
using Ucms.Domain.Exceptions;

public static class UploadIncomeFile
{
    public record Command(Guid IncomeId, IFormFile File);

    public sealed class Handler(IUcmsDbContext db, IFileStorageClient storageClient)
    {
        public async Task<(FileEntryModel? Result, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (cmd.File.ContentType != "application/pdf")
                return (null, "Fayl PDF formatida bo'lishi kerak");

            var income = await db.Incomes.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.IncomeId, ct);
            if (income is null) return (null, "Topilmadi");

            var response = await storageClient.UploadAsync("incomes", $"{cmd.IncomeId}.pdf", cmd.File.OpenReadStream(), ct)
                ?? throw new AppException("Faylni yuklashda xatolik yuz berdi. / Ошибка при загрузке файла.");

            income.FilePath = response.FilePath;
            await db.SaveChangesAsync(ct);
            return (response, null);
        }
    }
}
