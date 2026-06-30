namespace Ucms.Application.Features.Users.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class UpdateUser
{
    public record Command(Guid Id, string? FullName, string? PhoneNumber, string? Email);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, IEnumerable<string>? Errors)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(cmd.Id.ToString());
            if (user is null || user.IsDeleted) return (true, false, null);

            var canManage = ctx.IsOwner || (user.OrganizationId.HasValue && ctx.OrganizationId == user.OrganizationId);
            if (!canManage) return (false, true, null);

            if (cmd.FullName is not null)   user.FullName    = cmd.FullName;
            if (cmd.PhoneNumber is not null) user.PhoneNumber = cmd.PhoneNumber;
            if (cmd.Email is not null)
            {
                user.Email           = cmd.Email;
                user.NormalizedEmail = cmd.Email.ToUpperInvariant();
            }

            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = ctx.UserId ?? Guid.Empty;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, false, result.Errors.Select(e => e.Description));

            return (false, false, null);
        }
    }
}
