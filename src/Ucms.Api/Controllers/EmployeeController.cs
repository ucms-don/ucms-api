namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Employees.Commands;
using Ucms.Application.Features.Employees.Queries;

/// <summary>
/// Xodimlarni boshqarish.
/// Управление сотрудниками.
/// </summary>
[ApiController]
[Route("api/employees")]
[Tags("Employee")]
[Authorize]
public class EmployeeController(
    GetEmployees.Handler    getAll,
    GetEmployeeById.Handler getById,
    CreateEmployee.Handler  create,
    UpdateEmployee.Handler  update,
    DeleteEmployee.Handler  delete) : ControllerBase
{
    public record CreateEmployeeRequest(
        string Name, string? Position, string? Phone, string? Notes, Guid? BrigadeId, Guid? UserId);

    public record UpdateEmployeeRequest(
        string Name, string? Position, string? Phone, string? Notes, Guid? BrigadeId, Guid? UserId, bool IsActive);

    /// <summary>
    /// Xodimlar ro'yxati.
    /// Список сотрудников.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] Guid? brigadeId,
        CancellationToken ct = default)
    {
        return Ok(await getAll.HandleAsync(new(isActive, brigadeId), ct));
    }

    /// <summary>
    /// ID bo'yicha xodimni olish.
    /// Получить сотрудника по ID.
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
    /// Yangi xodim qo'shish. Admin yoki Manager uchun.
    /// Добавить нового сотрудника. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req, CancellationToken ct)
    {
        var result = await create.HandleAsync(new(req.Name, req.Position, req.Phone, req.Notes, req.BrigadeId, req.UserId), ct);
        if (result is null) return BadRequest(new { message = "Foydalanuvchiga tashkilot biriktirilmagan. / Пользователю не привязана организация." });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Xodim ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// Обновить данные сотрудника. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await update.HandleAsync(
            new(id, req.Name, req.Position, req.Phone, req.Notes, req.BrigadeId, req.UserId, req.IsActive), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Xodimni o'chirish. Admin yoki Manager uchun.
    /// Удалить сотрудника. Для Admin или Manager.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var (notFound, forbidden) = await delete.HandleAsync(new(id), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }
}
