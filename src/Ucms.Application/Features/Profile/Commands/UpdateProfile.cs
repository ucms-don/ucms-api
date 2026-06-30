namespace Ucms.Application.Features.Profile.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class UpdateProfile
{
    public record Command(string? FullName, string? PhoneNumber, string? Email);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(bool Unauthorized, IEnumerable<string>? Errors)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (ctx.UserId is null) return (true, null);

            var user = await userManager.FindByIdAsync(ctx.UserId.ToString()!);
            if (user is null || user.IsDeleted) return (true, null);

            if (cmd.FullName is not null)    user.FullName    = cmd.FullName;
            if (cmd.PhoneNumber is not null) user.PhoneNumber = cmd.PhoneNumber;
            if (cmd.Email is not null)
            {
                user.Email           = cmd.Email;
                user.NormalizedEmail = cmd.Email.ToUpperInvariant();
            }

            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = ctx.UserId.Value;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            return (false, null);
        }
    }
}
