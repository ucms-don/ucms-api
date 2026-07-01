namespace Ucms.Application.Abstractions;

/// <summary>
/// Joriy so'rov konteksti — foydalanuvchi va tashkilot ma'lumotlari
/// </summary>
public interface ICurrentContext
{
    /// <summary>
    /// Joriy foydalanuvchi ID (JWT sub claim)
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Joriy foydalanuvchi username
    /// </summary>
    public string? UserName { get; }

    /// <summary>
    /// Joriy foydalanuvchining tashkilot ID si
    /// </summary>
    public Guid? OrganizationId { get; }

    /// <summary>
    /// Joriy foydalanuvchining tashkilot nomi
    /// </summary>
    public string? OrganizationName { get; }

    /// <summary>
    /// Tizim admini ekanligini tekshirish (o'z tashkilotida Admin roli bor)
    /// </summary>
    public bool IsAdmin { get; }

    /// <summary>
    /// Tizim egasi tashkilotiga mansub foydalanuvchi.
    /// Owner foydalanuvchilar barcha tashkilotlar ma'lumotlarini ko'radi va boshqaradi.
    /// </summary>
    public bool IsOwner { get; }

    /// <summary>
    /// Foydalanuvchi rollari
    /// </summary>
    public IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Joriy UI tili: "uz" | "ru" | "en" | "ka"  (standart: "uz")
    /// Accept-Language sarlavhasidan olinadi.
    /// </summary>
    public string Locale { get; }
}
