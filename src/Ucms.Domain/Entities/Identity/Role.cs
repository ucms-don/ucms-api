namespace Ucms.Domain.Entities.Identity;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Tizim roli — ASP.NET Identity Role
/// </summary>
public class Role : IdentityRole<Guid>
{
    public Role() { }
    public Role(string roleName) : base(roleName) { }

    public string? Description { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}
