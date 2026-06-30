namespace Ucms.Domain.Entities.Identity;

using Ucms.Domain.Common;

/// <summary>
/// JWT Refresh Token
/// </summary>
public class RefreshToken : Entity
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Token qiymati (hashed yoki raw)
    /// </summary>
    public string Token { get; set; } = default!;

    /// <summary>
    /// Amal qilish muddati
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Bekor qilinganmi
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Yaratilgan sana
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Qaysi qurilmadan (User-Agent)
    /// </summary>
    public string? DeviceInfo { get; set; }

    public virtual User? User { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
