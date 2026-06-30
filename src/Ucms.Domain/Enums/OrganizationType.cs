namespace Ucms.Domain.Enums;

/// <summary>
/// Tashkilot turi — tizim egasimi yoki foydalanuvchimi
/// </summary>
public enum OrganizationType
{
    /// <summary>
    /// Tizim egasi — UCMS ni ishlab chiqqan kompaniya.
    /// Barcha tashkilotlar va ularning ma'lumotlariga to'liq kirish huquqi bor.
    /// </summary>
    Owner  = 1,

    /// <summary>
    /// Foydalanuvchi — UCMS dan foydalanadigan qurilish kompaniyasi.
    /// Faqat o'z tashkilotining ma'lumotlarini ko'ra oladi.
    /// </summary>
    Tenant = 2,
}
