namespace Ucms.Api.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// "permission" claim tipidagi qiymatni tekshirish talabi.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
