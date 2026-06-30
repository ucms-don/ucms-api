namespace Ucms.Infrastructure.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ucms.Application.Abstractions.Storage;

/// <summary>
/// Foydalanuvchi avatarlarini lokal diskka saqlaydi. Papka manzili
/// "Storage:AvatarsPath" sozlamasi orqali konfiguratsiya qilinadi (production'da
/// bu — Docker volume bilan saqlanadigan papka bo'lishi kerak, aks holda image
/// qayta build qilinganda fayllar yo'qoladi).
/// Сохраняет аватары пользователей локально на диск. Путь к папке настраивается
/// через "Storage:AvatarsPath" (в production должен указывать на смонтированный
/// Docker volume, иначе файлы пропадут при пересборке образа).
/// </summary>
public class LocalAvatarStorageClient(IWebHostEnvironment env, IConfiguration configuration) : IAvatarStorageClient
{
    public async Task<string> SaveAsync(Guid userId, string fileExtension, Stream stream, CancellationToken cancellationToken = default)
    {
        var avatarsDir = AvatarPathResolver.Resolve(configuration, env);
        Directory.CreateDirectory(avatarsDir);

        var fileName = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{fileExtension}";
        var filePath = Path.Combine(avatarsDir, fileName);

        await using var file = File.Create(filePath);
        await stream.CopyToAsync(file, cancellationToken);

        return $"/api/avatars/{fileName}";
    }

    public void DeleteExisting(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl)) return;

        var fileName = Path.GetFileName(avatarUrl);
        if (string.IsNullOrWhiteSpace(fileName)) return;

        var avatarsDir = AvatarPathResolver.Resolve(configuration, env);
        var filePath = Path.Combine(avatarsDir, fileName);
        if (File.Exists(filePath))
        {
            try { File.Delete(filePath); } catch { /* eskirgan faylni o'chirishda xato — e'tiborsiz qoldiramiz */ }
        }
    }
}
