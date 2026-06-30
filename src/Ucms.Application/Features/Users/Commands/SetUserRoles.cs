namespace Ucms.Application.Features.Users.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class SetUserRoles
{
    public record Command(Guid Id, List<string> Roles);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(cmd.Id.ToString());
            if (user is null || user.IsDeleted) return (true, false);

            var canManage = ctx.IsOwner || (user.OrganizationId.HasValue && ctx.OrganizationId == user.OrganizationId);
            if (!canManage) return (false, true);

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRolesAsync(user, cmd.Roles);
            return (false, false);
        }
    }
}
