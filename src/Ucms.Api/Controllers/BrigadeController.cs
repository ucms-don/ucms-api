namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Brigades.Commands;
using Ucms.Application.Features.Brigades.Queries;

/// <summary>
/// Brigadalarni boshqarish.
/// Управление бригадами.
/// </summary>
[ApiController]
[Route("api/brigades")]
[Tags("Brigade")]
[Authorize]
public class BrigadeController(
    GetBrigades.Handler              getAll,
    GetBrigadeById.Handler           getById,
    CreateBrigade.Handler            create,
    UpdateBrigade.Handler            update,
    DeleteBrigade.Handler            delete,
    AssignBrigadeEmployees.Handler   assignEmployees,
    RemoveBrigadeEmployee.Handler    removeEmployee) : ControllerBase
{
    public record CreateBrigadeRequest(string Name, string? ForemanName, string? Phone, string? Notes);

    public record UpdateBrigadeRequest(string Name, string? ForemanName, string? Phone, bool IsActive, string? Notes);

    public record AssignEmployeesRequest(Guid[] EmployeeIds);

    /// <summary>
    /// Brigadalar ro'yxati.
    /// Список бригад.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        return Ok(await getAll.HandleAsync(new(isActive, status), ct));
    }

    /// <summary>
    /// ID bo'yicha brigadani olish (xodimlar ro'yxati bilan).
    /// Получить бригаду по ID (со списком сотрудников).
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
    /// Yangi brigada yaratish. Admin yoki Manager uchun.
    /// Создать новую бригаду. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateBrigadeRequest req, CancellationToken ct)
    {
        var result = await create.HandleAsync(new(req.Name, req.ForemanName, req.Phone, req.Notes), ct);
        if (result is null) return BadRequest(new { message = "Foydalanuvchiga tashkilot biriktirilmagan. / Пользователю не привязана организация." });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Brigadani yangilash. Admin yoki Manager uchun.
    /// Обновить бригаду. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrigadeRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await update.HandleAsync(new(id, req.Name, req.ForemanName, req.Phone, req.IsActive, req.Notes), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Brigadani o'chirish. Admin yoki Manager uchun.
    /// Удалить бригаду. Для Admin или Manager.
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

    /// <summary>
    /// Xodimlarni brigadaga biriktirish. Admin yoki Manager uchun.
    /// Назначить сотрудников в бригаду. Для Admin или Manager.
    /// </summary>
    [HttpPost("{id:guid}/employees")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignEmployees(Guid id, [FromBody] AssignEmployeesRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, assigned) = await assignEmployees.HandleAsync(new(id, req.EmployeeIds), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(new { assigned });
    }

    /// <summary>
    /// Xodimni brigadadan chiqarish. Admin yoki Manager uchun.
    /// Убрать сотрудника из бригады. Для Admin или Manager.
    /// </summary>
    [HttpDelete("{id:guid}/employees/{employeeId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveEmployee(Guid id, Guid employeeId, CancellationToken ct)
    {
        var (notFound, forbidden) = await removeEmployee.HandleAsync(new(id, employeeId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }
}
