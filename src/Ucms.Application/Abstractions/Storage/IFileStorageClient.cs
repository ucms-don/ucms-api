namespace Ucms.Application.Abstractions.Storage;

public interface IFileStorageClient
{
    public Task<FileEntryModel?> UploadAsync(string path, string fileName, Stream stream, CancellationToken cancellationToken = default);
}
