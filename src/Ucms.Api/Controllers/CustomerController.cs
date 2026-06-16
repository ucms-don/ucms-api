namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Customers.Commands;
using Ucms.Application.Features.Customers.Queries;

/// <summary>
/// Buyurtmachilarni boshqarish.
/// Управление заказчиками.
/// </summary>
[ApiController]
[Route("api/customers")]
[Tags("Customer")]
[Authorize]
public class CustomerController(
    GetCustomers.Handler    getAll,
    GetCustomerById.Handler getById,
    CreateCustomer.Handler  create,
    UpdateCustomer.Handler  update,
    DeleteCustomer.Handler  delete) : ControllerBase
{
    public record CreateCustomerRequest(string Name, string? Phone, string? TaxId, string? Address, string? Notes);

    public record UpdateCustomerRequest(string Name, string? Phone, string? TaxId, string? Address, string? Notes, bool IsActive);

    /// <summary>
    /// Buyurtmachilar ro'yxati (qidiruv va faollik filtri bilan).
    /// Список заказчиков (с поиском и фильтром по активности).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(new(search, isActive, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha buyurtmachini olish (loyihalari bilan).
    /// Получить заказчика по ID (с проектами).
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
    /// Yangi buyurtmachi qo'shish. Admin yoki Manager uchun.
    /// Добавить нового заказчика. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req, CancellationToken ct)
    {
        var result = await create.HandleAsync(new(req.Name, req.Phone, req.TaxId, req.Address, req.Notes), ct);
        if (result is null) return BadRequest(new { message = "Foydalanuvchiga tashkilot biriktirilmagan. / Пользователю не привязана организация." });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Buyurtmachi ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// Обновить данные заказчика. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await update.HandleAsync(
            new(id, req.Name, req.Phone, req.TaxId, req.Address, req.Notes, req.IsActive), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Buyurtmachini o'chirish. Admin yoki Manager uchun.
    /// Удалить заказчика. Для Admin или Manager.
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
