namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.ClientActs.Commands;
using Ucms.Application.Features.ClientActs.DTOs;
using Ucms.Application.Features.ClientActs.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Loyiha aktlarini boshqarish.
/// Управление актами проекта.
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/acts")]
[Tags("ClientAct")]
[Authorize]
[Authorize(Policy = "projects.view")]
public class ClientActController(
    GetClientActs.Handler       getAll,
    GetClientActById.Handler    getById,
    CreateClientAct.Handler     create,
    UpdateClientActStatus.Handler updateStatus,
    DeleteClientAct.Handler     delete) : ControllerBase
{
    public record ActItemDto(Guid EstimateItemId, decimal Volume, decimal UnitPrice);

    public record CreateActRequest(
        string ActNumber, DateTimeOffset ActDate,
        List<ActItemDto> Items, string? Note);

    public record UpdateActStatusRequest(ActStatus Status);

    /// <summary>
    /// Loyiha aktlari ro'yxati.
    /// Список актов проекта.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll(
        Guid projectId, [FromQuery] ActStatus? status, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await getAll.HandleAsync(new(projectId, status), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha aktni olish.
    /// Получить акт по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientActDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid projectId, Guid id, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await getById.HandleAsync(new(projectId, id), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Yangi akt yaratish. Admin, Manager yoki Accountant uchun.
    /// Создать новый акт. Для Admin, Manager или Accountant.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Accountant")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create(
        Guid projectId, [FromBody] CreateActRequest req, CancellationToken ct)
    {
        var items = req.Items.Select(i =>
            new CreateClientAct.ActItemDto(i.EstimateItemId, i.Volume, i.UnitPrice)).ToList();

        var (data, notFound, forbidden) = await create.HandleAsync(
            new(projectId, req.ActNumber, req.ActDate, items, req.Note), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();

        // data handler tomonidan kafolatlanadi — notFound va forbidden false bo'lsa null kelmasligi kerak
        if (data is null) return StatusCode(500, new { message = "Akt yaratishda kutilmagan xato. / Непредвиденная ошибка при создании акта." });
        return CreatedAtAction(nameof(GetById), new { projectId, id = data.Id }, data);
    }

    /// <summary>
    /// Akt holatini yangilash. Admin, Manager yoki Accountant uchun.
    /// Обновить статус акта. Для Admin, Manager или Accountant.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,Accountant")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(
        Guid projectId, Guid id, [FromBody] UpdateActStatusRequest req, CancellationToken ct)
    {
        var (notFound, projectNotFound, forbidden) = await updateStatus.HandleAsync(
            new(projectId, id, req.Status), ct);
        if (projectNotFound) return NotFound();
        if (notFound)        return NotFound();
        if (forbidden)       return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Aktni o'chirish. Admin yoki Manager uchun.
    /// Удалить акт. Для Admin или Manager.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken ct)
    {
        var (notFound, projectNotFound, forbidden, error) = await delete.HandleAsync(new(projectId, id), ct);
        if (projectNotFound)   return NotFound();
        if (notFound)          return NotFound();
        if (forbidden)         return Forbid();
        if (error is not null) return BadRequest(new { message = error });
        return NoContent();
    }
}
