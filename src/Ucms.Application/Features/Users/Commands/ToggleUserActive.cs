namespace Ucms.Application.Features.Users.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class ToggleUserActive
{
    public record Command(Guid Id);

    public record Result(bool Active, string Message);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(cmd.Id.ToString());
            if (user is null || user.IsDeleted) return (null, true, false);

            var canManage = ctx.IsOwner || (user.OrganizationId.HasValue && ctx.OrganizationId == user.OrganizationId);
            if (!canManage) return (null, false, true);

            var isLocked = await userManager.IsLockedOutAsync(user);
            if (isLocked)
            {
                await userManager.SetLockoutEndDateAsync(user, null);
                return (new Result(true, "Foydalanuvchi faollashtirildi"), false, false);
            }
            else
            {
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                return (new Result(false, "Foydalanuvchi bloklandi"), false, false);
            }
        }
    }
}
