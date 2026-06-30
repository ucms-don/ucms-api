namespace Ucms.Infrastructure.Services;

using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Authorization;

/// <summary>
/// Tashqi permission servis mavjud bo'lmaguncha ishlatiladi.
/// Admin va Owner foydalanuvchilar uchun ruxsat beradi, boshqalar uchun rad etadi.
/// </summary>
public class StubPermissionProvider(ICurrentContext context) : IPermissionProvider
{
    public Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        var hasPermission = context.IsAdmin || context.IsOwner;
        return Task.FromResult(hasPermission);
    }
}
