namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.CashAccounts.Commands;
using Ucms.Application.Features.CashAccounts.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Kassa va bank hisoblarini boshqarish. Balans har doim CashTransaction'lardan hisoblanadi.
/// Управление кассами и банковскими счетами. Баланс всегда вычисляется из CashTransaction.
/// </summary>
[ApiController]
[Route("api/cash-accounts")]
[Tags("CashAccount")]
[Authorize]
public class CashAccountController(
    GetCashAccounts.Handler    getAll,
    GetCashAccountById.Handler getById,
    CreateCashAccount.Handler  create,
    UpdateCashAccount.Handler  update,
    DeleteCashAccount.Handler  delete) : ControllerBase
{
    public record CreateCashAccountRequest(string Name, CashAccountType Type, string? Notes);

    public record UpdateCashAccountRequest(string Name, CashAccountType Type, string? Notes, bool IsActive);

    /// <summary>
    /// Kassa/hisoblar ro'yxati (faollik va tur filtri bilan), umumiy balans bilan.
    /// Список касс/счетов (с фильтром по активности и типу), с общим балансом.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] CashAccountType? type,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(new(isActive, type), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha kassa/hisobni olish (balans va so'nggi harakatlar bilan).
    /// Получить кассу/счёт по ID (с балансом и последними операциями).
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
    /// Yangi kassa/hisob qo'shish. Admin yoki Manager uchun.
    /// Добавить новую кассу/счёт. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCashAccountRequest req, CancellationToken ct)
    {
        var result = await create.HandleAsync(new(req.Name, req.Type, req.Notes), ct);
        if (result is null) return BadRequest(new { message = "Foydalanuvchiga tashkilot biriktirilmagan. / Пользователю не привязана организация." });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Kassa/hisob ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// Обновить данные кассы/счёта. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCashAccountRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await update.HandleAsync(new(id, req.Name, req.Type, req.Notes, req.IsActive), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Kassa/hisobni o'chirish. Admin yoki Manager uchun.
    /// Удалить кассу/счёт. Для Admin или Manager.
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
