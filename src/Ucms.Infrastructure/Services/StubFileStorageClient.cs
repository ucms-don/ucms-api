namespace Ucms.Infrastructure.Services;

using Ucms.Application.Abstractions.Storage;

/// <summary>
/// Tashqi fayl saqlash servis mavjud bo'lmaguncha ishlatiladi.
/// Faylni local filesystem ga saqlaydi.
/// </summary>
public class StubFileStorageClient : IFileStorageClient
{
    public async Task<FileEntryModel?> UploadAsync(
        string path,
        string fileName,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        // TODO: MinIO, S3 yoki boshqa tashqi servis bilan almashtirish kerak
        var uploadDir = Path.Combine("uploads", path);
        Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, fileName);
        await using var file = File.Create(filePath);
        await stream.CopyToAsync(file, cancellationToken);

        return new FileEntryModel
        {
            FilePath = filePath,
            FileName = fileName,
            ContentType = "application/octet-stream"
        };
    }
}
