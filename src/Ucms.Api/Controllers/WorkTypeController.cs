namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.WorkTypes.Commands;
using Ucms.Application.Features.WorkTypes.Queries;
using Ucms.Application.Services;

/// <summary>
/// Ish turlarini boshqarish (smeta pozitsiyalari uchun sprvashnik).
/// Управление видами работ (справочник для позиций смет).
/// </summary>
[ApiController]
[Route("api/work-types")]
[Tags("Lookup")]
[Authorize]
[Authorize(Policy = "refs.view")]
public class WorkTypeController(
    GetWorkTypes.Handler    getAll,
    GetWorkTypeById.Handler getById,
    CreateWorkType.Handler  create,
    UpdateWorkType.Handler  update,
    DeleteWorkType.Handler  deleteOne,
    IWorkTypeCodeGenerator  codeGenerator) : ControllerBase
{
    public record CreateWorkTypeRequest(string Name, string NameRu, string? NameEn, string? NameKa,
        Guid? MeasurementUnitId, string? Code);
    public record UpdateWorkTypeRequest(string Name, string NameRu, string? NameEn, string? NameKa,
        Guid? MeasurementUnitId, string? Code);

    /// <summary>
    /// Barcha ish turlari ro'yxati.
    /// Список всех видов работ.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await getAll.HandleAsync(new(), ct));
    }

    /// <summary>
    /// ID bo'yicha ish turini olish.
    /// Получить вид работы по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Ish turi uchun noyob kod generatsiya qiladi (WT-000001 formatida).
    /// Генерирует уникальный код для вида работы (формат WT-000001).
    /// </summary>
    [HttpGet("generate-code")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> GenerateCode(CancellationToken ct)
    {
        var code = await codeGenerator.GenerateAsync(ct);
        return Ok(code);
    }

    /// <summary>
    /// Yangi ish turi yaratish. Faqat Admin uchun.
    /// Создать новый вид работы. Только для Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateWorkTypeRequest req, CancellationToken ct)
    {
        var (id, error) = await create.HandleAsync(new(req.Name, req.NameRu, req.NameEn, req.NameKa, req.MeasurementUnitId, req.Code), ct);
        if (error is not null) return Conflict(error);
        return StatusCode(201, id);
    }

    /// <summary>
    /// Ish turini yangilash. Faqat Admin uchun.
    /// Обновить вид работы. Только для Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkTypeRequest req, CancellationToken ct)
    {
        var (notFound, error) = await update.HandleAsync(new(id, req.Name, req.NameRu, req.NameEn, req.NameKa, req.MeasurementUnitId, req.Code), ct);
        if (notFound) return NotFound();
        if (error is not null) return Conflict(error);
        return NoContent();
    }

    /// <summary>
    /// Ish turini o'chirish (soft-delete). Faqat Admin uchun.
    /// Удалить вид работы (мягкое удаление). Только для Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var found = await deleteOne.HandleAsync(new(id), ct);
        return found ? NoContent() : NotFound();
    }
}
