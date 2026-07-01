namespace Ucms.Infrastructure.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ucms.Application.Abstractions;

public class HttpCurrentContext(IHttpContextAccessor httpContextAccessor) : ICurrentContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name)
                            ?? User?.FindFirstValue("name");

    public Guid? OrganizationId
    {
        get
        {
            var value = User?.FindFirstValue("organization_id")
                     ?? User?.FindFirstValue("org_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? OrganizationName => User?.FindFirstValue("organization_name");

    public bool IsAdmin => User?.IsInRole("Admin") ?? false;

    /// <summary>
    /// JWT dagi "org_type" claim "Owner" bo'lsa true qaytaradi.
    /// Owner foydalanuvchilar barcha tashkilotlar ma'lumotlarini boshqara oladi.
    /// </summary>
    public bool IsOwner =>
        string.Equals(
            User?.FindFirstValue("org_type"),
            "Owner",
            StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? (IReadOnlyList<string>)[];

    public string Locale
    {
        get
        {
            var header = httpContextAccessor.HttpContext?
                .Request.Headers["Accept-Language"].ToString() ?? string.Empty;

            // "ru-RU,ru;q=0.9,..." -> birinchi tag, primary subtag
            var primary = header.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault()
                                ?.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault()
                                ?.Split('-', StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault()
                                ?.ToLowerInvariant()
                         ?? "uz";

            return primary switch
            {
                "ru" => "ru",
                "en" => "en",
                "ka" => "ka",
                _    => "uz",
            };
        }
    }
}
