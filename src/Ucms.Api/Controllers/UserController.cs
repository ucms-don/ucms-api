namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Users.Commands;
using Ucms.Application.Features.Users.Queries;
using Ucms.Domain.Constants;

/// <summary>
/// Foydalanuvchilarni boshqarish.
/// Управление пользователями.
/// </summary>
[ApiController]
[Route("api/users")]
[Tags("User")]
[Authorize]
[Authorize(Policy = "personnel.view")]
public class UserController(
    GetUsers.Handler          getAll,
    GetUserById.Handler       getById,
    CreateUser.Handler        create,
    UpdateUser.Handler        update,
    SetUserRoles.Handler      setRoles,
    ResetUserPassword.Handler resetPassword,
    ToggleUserActive.Handler  toggleActive,
    DeleteUser.Handler        delete,
    GetRoles.Handler          getRoles) : ControllerBase
{
    public record CreateUserRequest(
        string UserName, string Email, string Password,
        string? FullName, string? PhoneNumber, List<string> Roles);

    public record UpdateUserRequest(string? FullName, string? PhoneNumber, string? Email);

    public record SetRolesRequest(List<string> Roles);

    public record ResetPasswordRequest(string NewPassword);

    /// <summary>
    /// Foydalanuvchilar ro'yxati (filtr va sahifalash bilan).
    /// Список пользователей (с фильтром и пагинацией).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? organizationId,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(new(organizationId, search, isActive, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha foydalanuvchini olish.
    /// Получить пользователя по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var (data, forbidden) = await getById.HandleAsync(new(id), ct);
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Yangi foydalanuvchi yaratish. Admin yoki Manager uchun.
    /// Создать нового пользователя. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        var (data, forbidden, errors) = await create.HandleAsync(
            new(req.UserName, req.Email, req.Password, req.FullName, req.PhoneNumber, req.Roles), ct);
        if (forbidden)          return BadRequest(new { message = "Foydalanuvchiga tashkilot biriktirilmagan. / Пользователю не привязана организация." });
        if (errors is not null) return BadRequest(new { errors });
        return StatusCode(201, data);
    }

    /// <summary>
    /// Foydalanuvchi ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// Обновить данные пользователя. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, errors) = await update.HandleAsync(
            new(id, req.FullName, req.PhoneNumber, req.Email), ct);
        if (notFound)           return NotFound();
        if (forbidden)          return Forbid();
        if (errors is not null) return BadRequest(new { errors });
        return NoContent();
    }

    /// <summary>
    /// Foydalanuvchiga rollarni belgilash. Admin yoki Manager uchun.
    /// Назначить роли пользователю. Для Admin или Manager.
    /// </summary>
    [HttpPatch("{id:guid}/roles")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetRolesRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await setRoles.HandleAsync(new(id, req.Roles), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(new { roles = req.Roles });
    }

    /// <summary>
    /// Foydalanuvchi parolini tiklash. Admin yoki Manager uchun.
    /// Сброс пароля пользователя. Для Admin или Manager.
    /// </summary>
    [HttpPatch("{id:guid}/reset-password")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, errors) = await resetPassword.HandleAsync(new(id, req.NewPassword), ct);
        if (notFound)           return NotFound();
        if (forbidden)          return Forbid();
        if (errors is not null) return BadRequest(new { errors });
        return NoContent();
    }

    /// <summary>
    /// Foydalanuvchi faolligini yoqish/o'chirish. Admin yoki Manager uchun.
    /// Включить/отключить активность пользователя. Для Admin или Manager.
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await toggleActive.HandleAsync(new(id), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Foydalanuvchini o'chirish. Admin yoki Manager uchun.
    /// Удалить пользователя. Для Admin или Manager.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var (notFound, forbidden, error) = await delete.HandleAsync(new(id), ct);
        if (notFound)          return NotFound();
        if (forbidden)         return Forbid();
        if (error is not null) return BadRequest(new { message = error });
        return NoContent();
    }

    /// <summary>
    /// Tizimda mavjud barcha rollar ro'yxati.
    /// Список всех доступных ролей в системе.
    /// </summary>
    [HttpGet("roles")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        return Ok(await getRoles.HandleAsync(new(), ct));
    }
}
