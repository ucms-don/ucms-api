namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.MeasurementUnits.Commands;
using Ucms.Application.Features.MeasurementUnits.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// O'lchov birliklarini boshqarish.
/// Управление единицами измерения.
/// </summary>
[ApiController]
[Route("api/measurement-units")]
[Tags("Lookup")]
[Authorize]
[Authorize(Policy = "refs.view")]
public class MeasurementUnitController(
    GetMeasurementUnits.Handler     getAll,
    GetFilteredMeasurementUnits.Handler getFiltered,
    GetMeasurementUnitById.Handler  getById,
    FindMeasurementUnit.Handler     findByCode,
    CreateMeasurementUnit.Handler   create,
    UpdateMeasurementUnit.Handler   update,
    DeleteMeasurementUnit.Handler   deleteOne,
    DeleteMeasurementUnits.Handler  deleteBulk) : ControllerBase
{
    public record CreateUnitRequest(string Code, string Name, string NameRu,
        string? NameEn, string? NameKa, MeasurementUnitType Type, decimal Multiplier = 1);
    public record UpdateUnitRequest(string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, MeasurementUnitType Type, decimal Multiplier);
    public record DeleteBulkRequest(Guid[] Ids);

    /// <summary>
    /// Barcha o'lchov birliklari ro'yxati.
    /// Список всех единиц измерения.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll([FromQuery] MeasurementUnitType? type, CancellationToken ct)
    {
        return Ok(await getAll.HandleAsync(new(type), ct));
    }

    /// <summary>
    /// Filtrланган o'lchov birliklari ro'yxati.
    /// Фильтрованный список единиц измерения.
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] string? search, [FromQuery] MeasurementUnitType? type,
        [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
    {
        return Ok(await getFiltered.HandleAsync(new(search, type, page, size), ct));
    }

    /// <summary>
    /// ID bo'yicha o'lchov birligini olish.
    /// Получить единицу измерения по ID.
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
    /// Kod bo'yicha o'lchov birligini qidirish.
    /// Найти единицу измерения по коду.
    /// </summary>
    [HttpGet("find/{code}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> FindByCode(string code, CancellationToken ct)
    {
        var result = await findByCode.HandleAsync(new(code), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Yangi o'lchov birligi yaratish. Faqat Admin uchun.
    /// Создать новую единицу измерения. Только для Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest req, CancellationToken ct)
    {
        var (id, error) = await create.HandleAsync(
            new(req.Code, req.Name, req.NameRu, req.NameEn, req.NameKa, req.Type, req.Multiplier), ct);
        if (error is not null) return Conflict(error);
        return StatusCode(201, id);
    }

    /// <summary>
    /// O'lchov birligini yangilash. Faqat Admin uchun.
    /// Обновить единицу измерения. Только для Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitRequest req, CancellationToken ct)
    {
        var (notFound, error) = await update.HandleAsync(
            new(id, req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code, req.Type, req.Multiplier), ct);
        if (notFound) return NotFound();
        if (error is not null) return Conflict(error);
        return NoContent();
    }

    /// <summary>
    /// O'lchov birligini o'chirish. Faqat Admin uchun.
    /// Удалить единицу измерения. Только для Admin.
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

    /// <summary>
    /// Bir nechta o'lchov birligini o'chirish. Faqat Admin uchun.
    /// Удалить несколько единиц измерения. Только для Admin.
    /// </summary>
    [HttpPost("delete-bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeleteBulk([FromBody] DeleteBulkRequest req, CancellationToken ct)
    {
        var result = await deleteBulk.HandleAsync(new(req.Ids), ct);

        return result ? NoContent() : Conflict(result);
    }
}
