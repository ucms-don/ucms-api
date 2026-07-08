namespace Ucms.Application.Features.Users.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class ResetUserPassword
{
    public record Command(Guid Id, string NewPassword);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, IEnumerable<string>? Errors)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(cmd.Id.ToString());
            if (user is null || user.IsDeleted) return (true, false, null);

            var canManage = ctx.IsOwner || (user.OrganizationId.HasValue && ctx.OrganizationId == user.OrganizationId);
            if (!canManage) return (false, true, null);

            var token  = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, cmd.NewPassword);
            if (!result.Succeeded)
                return (false, false, result.Errors.Select(e => e.Description));

            return (false, false, null);
        }
    }
}
