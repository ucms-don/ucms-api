namespace Ucms.Infrastructure.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Avatarlar saqlanadigan jisman papkani aniqlaydi.
/// "Storage:AvatarsPath" sozlamasi orqali konteynerdan tashqaridagi (volume bilan
/// saqlanadigan) joyga ko'rsatish mumkin — shunda image qayta build qilinganda
/// fayllar yo'qolmaydi.
/// Определяет физическую папку для аватаров. Через настройку "Storage:AvatarsPath"
/// можно указать путь вне контейнера (на смонтированном volume), чтобы файлы не
/// терялись при пересборке образа.
/// </summary>
public static class AvatarPathResolver
{
    public static string Resolve(IConfiguration configuration, IWebHostEnvironment env)
    {
        var configuredPath = configuration["Storage:AvatarsPath"];

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            var webRoot = string.IsNullOrEmpty(env.WebRootPath)
                ? Path.Combine(env.ContentRootPath, "wwwroot")
                : env.WebRootPath;

            return Path.Combine(webRoot, "avatars");
        }

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);
    }
}
