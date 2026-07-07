namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Features.Manufacturers.Commands;
using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Application.Features.Manufacturers.Queries;

/// <summary>
/// Ishlab chiqaruvchilarni boshqarish.
/// Управление производителями.
/// </summary>
[Route("api/manufacturers")]
[ApiController]
[Authorize]
[Authorize(Policy = "refs.view")]
public class ManufacturerController(
    GetManufacturers.Handler getAll,
    GetStockSkuManufacturers.Handler getStockSku,
    GetFilteredManufacturers.Handler getFiltered,
    GetManufacturerById.Handler getById,
    FindManufacturerByCode.Handler findByCode,
    FindManufacturerByName.Handler findByName,
    CreateManufacturer.Handler create,
    UpdateManufacturer.Handler update,
    DeleteManufacturer.Handler delete,
    DeleteManufacturers.Handler deleteRange) : ControllerBase
{
    /// <summary>
    /// Barcha ishlab chiqaruvchilar ro'yxati (sahifalash bilan).
    /// Список всех производителей (с пагинацией).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ManufacturerModel>), 200)]
    public async Task<IActionResult> GetManufacturers([FromQuery] string? query,
        [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            return Ok(await getAll.HandleAsync(new(query, page, size), ct));
        }

    /// <summary>
    /// Ombordagi SKU bo'yicha ishlab chiqaruvchilar.
    /// Производители по SKU на складе.
    /// </summary>
    [HttpGet("stock-sku")]
    [ProducesResponseType(typeof(PagedResult<ManufacturerModel>), 200)]
    public async Task<IActionResult> GetStockSkuManufacturers([FromQuery] string? query,
        [FromQuery] Guid? organizationId, [FromQuery] Guid? stockId, [FromQuery] Guid? productId,
        [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            return Ok(await getStockSku.HandleAsync(new(query, organizationId, stockId, productId, page, size), ct));
        }

    /// <summary>
    /// Filtrланган jadval ro'yxati.
    /// Фильтрованный табличный список.
    /// </summary>
    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<ManufacturerModel>), 200)]
    public async Task<IActionResult> SearchManufacturers([FromBody] PagedRequest filter, CancellationToken ct = default)
        {
            return Ok(await getFiltered.HandleAsync(new(filter), ct));
        }

    /// <summary>
    /// ID bo'yicha ishlab chiqaruvchini olish.
    /// Получить производителя по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ManufacturerModel), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetManufacturer(Guid id, CancellationToken ct = default)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Kod bo'yicha ishlab chiqaruvchini qidirish.
    /// Найти производителя по коду.
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> FindByCode(string code, CancellationToken ct = default)
        {
            return Ok(await findByCode.HandleAsync(new(code), ct));
        }

    /// <summary>
    /// Nom bo'yicha ishlab chiqaruvchini qidirish.
    /// Найти производителя по наименованию.
    /// </summary>
    [HttpGet("name/{name}")]
    public async Task<IActionResult> FindByName(string name, CancellationToken ct = default)
        {
            return Ok(await findByName.HandleAsync(new(name), ct));
        }

    public record CreateManufacturerRequest(string Name, string NameRu, string? NameEn, string? NameKa, string? Code);

    /// <summary>
    /// Yangi ishlab chiqaruvchi yaratish.
    /// Создать нового производителя.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateManufacturer([FromBody] CreateManufacturerRequest req, CancellationToken ct = default)
    {
        var (id, error) = await create.HandleAsync(new(req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code), ct);
        if (error is not null) return Conflict(error);
        return StatusCode(201, id);
    }

    public record UpdateManufacturerRequest(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa, string? Code);

    /// <summary>
    /// Ishlab chiqaruvchi ma'lumotlarini yangilash.
    /// Обновить данные производителя.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateManufacturer([FromBody] UpdateManufacturerRequest req, CancellationToken ct = default)
    {
        var ok = await update.HandleAsync(new(req.Id, req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code), ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Ishlab chiqaruvchini o'chirish.
    /// Удалить производителя.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeleteManufacturer(Guid id, CancellationToken ct = default)
    {
        var (notFound, error) = await delete.HandleAsync(new(id), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : NoContent();
    }

    /// <summary>
    /// Bir nechta ishlab chiqaruvchini o'chirish.
    /// Удалить несколько производителей.
    /// </summary>
    [HttpPost("delete-range")]
    [ProducesResponseType(204)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeleteManufacturers([FromBody] Guid[] ids, CancellationToken ct = default)
    {
        var (_, error) = await deleteRange.HandleAsync(new(ids), ct);
        return error is not null ? Conflict(error) : NoContent();
    }
}
