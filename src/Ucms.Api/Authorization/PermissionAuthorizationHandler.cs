namespace Ucms.Api.Authorization;

using Microsoft.AspNetCore.Authorization;
using Ucms.Domain.Constants;

/// <summary>
/// JWT dagi "permission" claimlarini tekshiradi.
/// Admin roli bo'lsa — barcha permissionslar avtomatik beriladi.
/// </summary>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement       requirement)
    {
        // Admin roli har qanday permissionni o'tkazib yuboradi
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasClaim = context.User.Claims.Any(c =>
            c.Type  == Permissions.ClaimType &&
            c.Value == requirement.Permission);

        if (hasClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
