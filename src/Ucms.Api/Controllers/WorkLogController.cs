namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.WorkLogs.Commands;
using Ucms.Application.Features.WorkLogs.DTOs;
using Ucms.Application.Features.WorkLogs.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Ish jurnallarini boshqarish.
/// Управление журналами работ.
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/worklogs")]
[Tags("WorkLog")]
[Authorize]
public class WorkLogController(
    GetWorkLogs.Handler       getAll,
    GetWorkLogById.Handler    getById,
    GetWorkLogSummary.Handler getSummary,
    CreateWorkLog.Handler     create,
    UpdateWorkLog.Handler     update,
    ConfirmWorkLogs.Handler   confirm,
    RejectWorkLogs.Handler    reject,
    DeleteWorkLog.Handler     delete) : ControllerBase
{
    public record CreateWorkLogRequest(
        Guid BrigadeId, Guid EstimateItemId,
        DateTimeOffset Date, decimal Volume,
        decimal? BrigadeUnitPrice,
        string? Floor, string? Zone, string? Room,
        string? Note);

    public record UpdateWorkLogRequest(
        DateTimeOffset Date, decimal Volume, decimal BrigadeUnitPrice,
        string? Floor, string? Zone, string? Room,
        string? Note);

    public record ConfirmWorkLogRequest(Guid[] WorkLogIds);
    public record RejectWorkLogRequest(Guid[] WorkLogIds, string? Reason);

    /// <summary>
    /// Loyiha ish jurnallari ro'yxati (filtr va sahifalash bilan).
    /// Список журналов работ проекта (с фильтром и пагинацией).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WorkLogPagedResult), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll(
        Guid projectId,
        [FromQuery] Guid? brigadeId,
        [FromQuery] WorkLogStatus? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, notFound, forbidden) = await getAll.HandleAsync(
            new(projectId, brigadeId, status, from, to, page, size), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha ish jurnalini olish.
    /// Получить журнал работ по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkLogDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid projectId, Guid id, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await getById.HandleAsync(new(projectId, id), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Loyiha ish jurnali xulosasi.
    /// Сводка по журналу работ проекта.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSummary(Guid projectId, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await getSummary.HandleAsync(new(projectId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Yangi ish jurnali yozuvi yaratish. Admin, Manager yoki Brigadir uchun.
    /// Создать новую запись журнала работ. Для Admin, Manager или Brigadir.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Brigadir")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create(
        Guid projectId, [FromBody] CreateWorkLogRequest req, CancellationToken ct)
    {
        var (data, notFound, forbidden, error) = await create.HandleAsync(
            new(projectId, req.BrigadeId, req.EstimateItemId, req.Date,
                req.Volume, req.BrigadeUnitPrice, req.Floor, req.Zone, req.Room, req.Note), ct);
        if (notFound)          return NotFound();
        if (forbidden)         return Forbid();
        if (error is not null) return BadRequest(new { message = error });
        return StatusCode(201, data);
    }

    /// <summary>
    /// Ish jurnali yozuvini yangilash. Admin, Manager yoki Brigadir uchun.
    /// Обновить запись журнала работ. Для Admin, Manager или Brigadir.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Brigadir")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid projectId, Guid id, [FromBody] UpdateWorkLogRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, error) = await update.HandleAsync(
            new(projectId, id, req.Date, req.Volume, req.BrigadeUnitPrice,
                req.Floor, req.Zone, req.Room, req.Note), ct);
        if (notFound)          return NotFound();
        if (forbidden)         return Forbid();
        if (error is not null) return BadRequest(new { message = error });
        return NoContent();
    }

    /// <summary>
    /// Ish jurnali yozuvlarini tasdiqlash. Admin yoki Manager uchun.
    /// Подтвердить записи журнала работ. Для Admin или Manager.
    /// </summary>
    [HttpPost("confirm")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Confirm(
        Guid projectId, [FromBody] ConfirmWorkLogRequest req, CancellationToken ct)
    {
        var (confirmed, notFound, forbidden) = await confirm.HandleAsync(
            new(projectId, req.WorkLogIds), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(new { confirmed });
    }

    /// <summary>
    /// Ish jurnali yozuvlarini rad etish. Admin yoki Manager uchun.
    /// Отклонить записи журнала работ. Для Admin или Manager.
    /// </summary>
    [HttpPost("reject")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reject(
        Guid projectId, [FromBody] RejectWorkLogRequest req, CancellationToken ct)
    {
        var (rejected, notFound, forbidden) = await reject.HandleAsync(
            new(projectId, req.WorkLogIds, req.Reason), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(new { rejected });
    }

    /// <summary>
    /// Ish jurnali yozuvini o'chirish. Admin yoki Manager uchun.
    /// Удалить запись журнала работ. Для Admin или Manager.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken ct)
    {
        var (notFound, error) = await delete.HandleAsync(new(projectId, id), ct);
        if (notFound)          return NotFound();
        if (error is not null) return BadRequest(new { message = error });
        return NoContent();
    }
}
