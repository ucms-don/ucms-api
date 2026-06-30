namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Projects.Commands;
using Ucms.Application.Features.Projects.DTOs;
using Ucms.Application.Features.Projects.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Loyihalarni boshqarish.
/// Управление проектами.
/// </summary>
[ApiController]
[Route("api/projects")]
[Tags("Project")]
[Authorize]
public class ProjectController(
    GetProjects.Handler    getAll,
    GetProjectById.Handler getById,
    CreateProject.Handler  create,
    UpdateProject.Handler  update,
    DeleteProject.Handler  delete) : ControllerBase
{
    public record CreateProjectRequest(
        string Name,
        string? Address,
        string? Description,
        string? ContractNumber,
        DateTimeOffset? ContractDate,
        DateTimeOffset? StartDate,
        DateTimeOffset? EndDate,
        decimal? ContractValue,
        Guid? CustomerId);

    public record UpdateProjectRequest(
        string Name,
        string? Address,
        string? Description,
        string? ContractNumber,
        DateTimeOffset? ContractDate,
        DateTimeOffset? StartDate,
        DateTimeOffset? EndDate,
        decimal? ContractValue,
        string? Status,
        Guid? CustomerId);

    private static ProjectStatus MapStatusStringToEnum(string? status) =>
        (status ?? string.Empty).ToLowerInvariant() switch
        {
            "planning"  => ProjectStatus.Planning,
            "active"    => ProjectStatus.InProgress,
            "completed" => ProjectStatus.Completed,
            "suspended" => ProjectStatus.Suspended,
            "archived"  => ProjectStatus.Cancelled,
            _           => ProjectStatus.Planning,
        };

    /// <summary>
    /// Loyihalar ro'yxati (sahifalash va holat filtri bilan).
    /// Список проектов (с пагинацией и фильтром по статусу).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ProjectStatus? status,
        [FromQuery] string? statusString,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(new(status, statusString, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha loyihani olish.
    /// Получить проект по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var (data, forbidden) = await getById.HandleAsync(new(id), ct);
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Yangi loyiha yaratish. Admin yoki Manager uchun.
    /// Создать новый проект. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req, CancellationToken ct)
    {
        var result = await create.HandleAsync(
            new(req.Name, req.Address, req.Description, req.ContractNumber,
                req.ContractDate, req.StartDate, req.EndDate, req.ContractValue, req.CustomerId), ct);

        if (result is null) return BadRequest(new { message = "Foydalanuvchiga tashkilot biriktirilmagan yoki buyurtmachi topilmadi. / Пользователю не привязана организация, или заказчик не найден." });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Loyiha ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// Обновить данные проекта. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, customerNotFound) = await update.HandleAsync(
            new(id, req.Name, req.Address, req.Description, req.ContractNumber,
                req.ContractDate, req.StartDate, req.EndDate, req.ContractValue,
                MapStatusStringToEnum(req.Status), req.CustomerId), ct);

        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        if (customerNotFound) return BadRequest(new { message = "Buyurtmachi topilmadi. / Заказчик не найден." });
        return NoContent();
    }

    /// <summary>
    /// Loyihani o'chirish. Admin yoki Manager uchun.
    /// Удалить проект. Для Admin или Manager.
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
