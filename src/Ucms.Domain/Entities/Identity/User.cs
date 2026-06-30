namespace Ucms.Domain.Entities.Identity;

using Microsoft.AspNetCore.Identity;
using Ucms.Domain.Common;

/// <summary>
/// Tizim foydalanuvchisi — ASP.NET Identity User
/// </summary>
public class User : IdentityUser<Guid>, IAuditableEntity, IDeletable
{
    /// <summary>
    /// To'liq ismi
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Profil rasmi URL manzili (nullable)
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Bog'liq tashkilot (nullable — admin uchun)
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// Bog'liq xodim yozuvi (nullable)
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// O'chirilgan yoki yo'q (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }

    // IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
