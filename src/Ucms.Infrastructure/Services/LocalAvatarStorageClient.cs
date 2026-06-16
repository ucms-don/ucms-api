namespace Ucms.Infrastructure.Services;

using Microsoft.AspNetCore.Hosting;
using Ucms.Application.Abstractions.Storage;

/// <summary>
/// Foydalanuvchi avatarlarini lokal wwwroot/avatars papkasiga saqlaydi.
/// Сохраняет аватары пользователей локально в wwwroot/avatars.
/// </summary>
public class LocalAvatarStorageClient(IWebHostEnvironment env) : IAvatarStorageClient
{
    private const string FolderName = "avatars";

    public async Task<string> SaveAsync(Guid userId, string fileExtension, Stream stream, CancellationToken cancellationToken = default)
    {
        var webRoot = string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;

        var avatarsDir = Path.Combine(webRoot, FolderName);
        Directory.CreateDirectory(avatarsDir);

        var fileName = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{fileExtension}";
        var filePath = Path.Combine(avatarsDir, fileName);

        await using var file = File.Create(filePath);
        await stream.CopyToAsync(file, cancellationToken);

        return $"/api/{FolderName}/{fileName}";
    }

    public void DeleteExisting(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl)) return;

        var fileName = Path.GetFileName(avatarUrl);
        if (string.IsNullOrWhiteSpace(fileName)) return;

        var webRoot = string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;

        var filePath = Path.Combine(webRoot, FolderName, fileName);
        if (File.Exists(filePath))
        {
            try { File.Delete(filePath); } catch { /* eskirgan faylni o'chirishda xato — e'tiborsiz qoldiramiz */ }
        }
    }
}
