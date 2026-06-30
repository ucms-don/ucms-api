namespace Ucms.Application.Features.Users.Commands;

using Microsoft.AspNetCore.Identity;
using Ucms.Application.Abstractions;
using Ucms.Domain.Entities.Identity;

public static class CreateUser
{
    public record Command(
        string UserName, string Email, string Password,
        string? FullName, string? PhoneNumber, List<string> Roles);

    public record Result(Guid Id, string UserName, string? Email);

    public sealed class Handler(UserManager<User> userManager, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden, IEnumerable<string>? Errors)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = ctx.IsOwner ? (Guid?)null : ctx.OrganizationId;
            if (!ctx.IsOwner && !orgId.HasValue) return (null, true, null);

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var user = new User
            {
                Id              = Guid.NewGuid(),
                UserName        = cmd.UserName,
                Email           = cmd.Email,
                NormalizedEmail = cmd.Email.ToUpperInvariant(),
                FullName        = cmd.FullName,
                PhoneNumber     = cmd.PhoneNumber,
                OrganizationId  = orgId,
                IsDeleted       = false,
                CreatedAt       = now, UpdatedAt = now,
                CreatedBy       = userId, UpdatedBy = userId,
            };

            var result = await userManager.CreateAsync(user, cmd.Password);
            if (!result.Succeeded)
                return (null, false, result.Errors.Select(e => e.Description));

            if (cmd.Roles.Count > 0)
                await userManager.AddToRolesAsync(user, cmd.Roles);

            return (new Result(user.Id, user.UserName!, user.Email), false, null);
        }
    }
}
