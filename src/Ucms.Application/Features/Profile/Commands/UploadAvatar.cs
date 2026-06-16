namespace Ucms.Application.Features.Profile.Commands;

using Microsoft.AspNetCore.Http;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Storage;
using Ucms.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

public static class UploadAvatar
{
    private static readonly Dictionary<string, string> AllowedTypes = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"]  = ".png",
        ["image/webp"] = ".webp",
    };

    public record Command(IFormFile File);

    public sealed class Handler(
        UserManager<User> userManager,
        ICurrentContext ctx,
        IAvatarStorageClient storage)
    {
        public async Task<(string? AvatarUrl, bool Unauthorized, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (ctx.UserId is null) return (null, true, null);

            if (cmd.File is null || cmd.File.Length == 0)
                return (null, false, "Fayl tanlanmagan");

            if (cmd.File.Length > 5 * 1024 * 1024)
                return (null, false, "Fayl hajmi 5 MB dan oshmasligi kerak");

            if (!AllowedTypes.TryGetValue(cmd.File.ContentType, out var ext))
                return (null, false, "Faqat JPEG, PNG yoki WEBP formatdagi rasm yuklash mumkin");

            var user = await userManager.FindByIdAsync(ctx.UserId.ToString()!);
            if (user is null || user.IsDeleted) return (null, true, null);

            var oldAvatarUrl = user.AvatarUrl;

            string newUrl;
            await using (var stream = cmd.File.OpenReadStream())
            {
                newUrl = await storage.SaveAsync(user.Id, ext, stream, ct);
            }

            user.AvatarUrl = newUrl;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = ctx.UserId.Value;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (null, false, string.Join("; ", result.Errors.Select(e => e.Description)));

            storage.DeleteExisting(oldAvatarUrl);

            return (newUrl, false, null);
        }
    }
}
