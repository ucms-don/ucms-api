namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Profile.Commands;
using Ucms.Application.Features.Profile.Queries;

/// <summary>
/// Joriy foydalanuvchi profilini boshqarish.
/// Управление профилем текущего пользователя.
/// </summary>
[ApiController]
[Route("api/profile")]
[Tags("Profile")]
[Authorize]
public class ProfileController(
    GetProfile.Handler     getProfile,
    UpdateProfile.Handler  updateProfile,
    ChangePassword.Handler changePassword,
    UploadAvatar.Handler   uploadAvatar) : ControllerBase
{
    public record UpdateProfileRequest(string? FullName, string? PhoneNumber, string? Email);

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    /// <summary>
    /// Joriy foydalanuvchi profil ma'lumotlarini olish.
    /// Получить данные профиля текущего пользователя.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var data = await getProfile.HandleAsync(new(), ct);
        return data is null ? Unauthorized() : Ok(data);
    }

    /// <summary>
    /// Joriy foydalanuvchi profil ma'lumotlarini yangilash.
    /// Обновить данные профиля текущего пользователя.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req, CancellationToken ct)
    {
        var (unauthorized, errors) = await updateProfile.HandleAsync(
            new(req.FullName, req.PhoneNumber, req.Email), ct);
        if (unauthorized)       return Unauthorized();
        if (errors is not null) return BadRequest(new { errors });
        return NoContent();
    }

    /// <summary>
    /// Parolni o'zgartirish.
    /// Изменить пароль.
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        var (unauthorized, errors) = await changePassword.HandleAsync(
            new(req.CurrentPassword, req.NewPassword), ct);
        if (unauthorized)       return Unauthorized();
        if (errors is not null) return BadRequest(new { errors });
        return Ok(new { message = "Parol muvaffaqiyatli o'zgartirildi. / Пароль успешно изменён." });
    }

    /// <summary>
    /// Profil rasmini (avatar) yuklash (maks. 5 MB, JPEG/PNG/WEBP).
    /// Загрузить аватар профиля (макс. 5 МБ, JPEG/PNG/WEBP).
    /// </summary>
    [HttpPost("avatar")]
    [RequestSizeLimit(5L * 1024L * 1024L)]
    [RequestFormLimits(MultipartBodyLengthLimit = 5L * 1024L * 1024L)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        var (avatarUrl, unauthorized, error) = await uploadAvatar.HandleAsync(new(file), ct);
        if (unauthorized)     return Unauthorized();
        if (error is not null) return BadRequest(new { message = error });
        return Ok(new { avatarUrl });
    }
}
