namespace Ucms.Application.Features.Profile.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class ChangePassword
{
    public record Command(string CurrentPassword, string NewPassword);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(bool Unauthorized, IEnumerable<string>? Errors)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (ctx.UserId is null) return (true, null);

            var user = await userManager.FindByIdAsync(ctx.UserId.ToString()!);
            if (user is null || user.IsDeleted) return (true, null);

            var result = await userManager.ChangePasswordAsync(user, cmd.CurrentPassword, cmd.NewPassword);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            return (false, null);
        }
    }
}
