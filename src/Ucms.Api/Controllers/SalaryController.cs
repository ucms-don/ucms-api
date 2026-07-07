namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Salaries.Commands;
using Ucms.Application.Features.Salaries.Queries;
using Ucms.Domain.Constants;

/// <summary>
/// Xodimlar maoshlarini boshqarish.
/// Управление зарплатами сотрудников.
/// </summary>
[ApiController]
[Route("api/salaries")]
[Tags("Salary")]
[Authorize]
[Authorize(Policy = "personnel.view")]
public class SalaryController(
    GetSalaries.Handler    getAll,
    GetSalaryById.Handler  getById,
    CreateSalary.Handler   create,
    UpdateSalary.Handler   update,
    DeleteSalary.Handler   delete,
    CancelSalary.Handler   cancel) : ControllerBase
{
    public record CreateSalaryRequest(
        Guid EmployeeId, string Month, decimal Amount, string? Notes, Guid CashAccountId);

    public record UpdateSalaryRequest(
        Guid EmployeeId, string Month, decimal Amount, string? Notes, Guid CashAccountId);

    /// <summary>
    /// Maoshlar ro'yxati (oy va xodim filtri bilan).
    /// Список зарплат (с фильтром по месяцу и сотруднику).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? month,
        [FromQuery] Guid? employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(new(month, employeeId, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha maosh yozuvini olish.
    /// Получить запись о зарплате по ID.
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
    /// Yangi maosh yozuvi qo'shish. Admin yoki Manager uchun.
    /// Добавить новую запись о зарплате. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateSalaryRequest req, CancellationToken ct)
    {
        var (data, notFound, forbidden, cashAccountNotFound, insufficientBalance) = await create.HandleAsync(
            new(req.EmployeeId, req.Month, req.Amount, req.Notes, req.CashAccountId), ct);
        if (notFound)  return NotFound(new { message = "Xodim topilmadi. / Сотрудник не найден." });
        if (forbidden) return Forbid();
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        if (insufficientBalance) return BadRequest(new { message = "Kassada mablag' yetarli emas. / Недостаточно средств на счёте." });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
    }

    /// <summary>
    /// Maosh yozuvini yangilash. Admin yoki Manager uchun.
    /// Обновить запись о зарплате. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalaryRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, error, cashAccountNotFound, insufficientBalance) = await update.HandleAsync(
            new(id, req.EmployeeId, req.Month, req.Amount, req.Notes, req.CashAccountId), ct);
        if (notFound)          return NotFound();
        if (forbidden)         return Forbid();
        if (error is not null) return BadRequest(new { message = error });
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        if (insufficientBalance) return BadRequest(new { message = "Kassada mablag' yetarli emas. / Недостаточно средств на счёте." });
        return NoContent();
    }

    /// <summary>
    /// Maosh yozuvini o'chirish. Admin yoki Manager uchun.
    /// Удалить запись о зарплате. Для Admin или Manager.
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

    /// <summary>Maosh yozuvini bekor qilish. Kassa balansi tiklanadi.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.Finance.Cancel)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var (notFound, forbidden, alreadyCancelled) = await cancel.HandleAsync(new(id), ct);
        if (notFound)         return NotFound(new { message = "Maosh yozuvi topilmadi." });
        if (forbidden)        return Forbid();
        if (alreadyCancelled) return Conflict(new { message = "Maosh yozuvi allaqachon bekor qilingan." });
        return NoContent();
    }
}
