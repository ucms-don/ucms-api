namespace Ucms.Application.Features.Users.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class DeleteUser
{
    public record Command(Guid Id);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(cmd.Id.ToString());
            if (user is null || user.IsDeleted) return (true, false, null);

            var canManage = ctx.IsOwner || (user.OrganizationId.HasValue && ctx.OrganizationId == user.OrganizationId);
            if (!canManage) return (false, true, null);

            if (user.Id == ctx.UserId)
                return (false, false, "O'zingizni o'chira olmaysiz");

            user.IsDeleted = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = ctx.UserId ?? Guid.Empty;

            await userManager.UpdateAsync(user);
            return (false, false, null);
        }
    }
}
