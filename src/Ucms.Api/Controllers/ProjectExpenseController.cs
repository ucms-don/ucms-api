namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.ProjectExpenses.Commands;
using Ucms.Application.Features.ProjectExpenses.Queries;

/// <summary>
/// Loyiha xarajatlarini boshqarish (materiallar, transport, boshqalar).
/// Управление расходами по проекту (материалы, транспорт, прочее).
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/expenses")]
[Tags("ProjectExpense")]
[Authorize]
public class ProjectExpenseController(
    GetProjectExpenses.Handler    getAll,
    GetProjectExpenseById.Handler getById,
    CreateProjectExpense.Handler  create,
    UpdateProjectExpense.Handler  update,
    DeleteProjectExpense.Handler  delete) : ControllerBase
{
    public record CreateExpenseRequest(
        DateTimeOffset Date, string Category, decimal Amount,
        string? Description, string? PaymentMethod, string? Note, Guid CashAccountId);

    public record UpdateExpenseRequest(
        DateTimeOffset Date, string Category, decimal Amount,
        string? Description, string? PaymentMethod, string? Note, Guid CashAccountId);

    /// <summary>
    /// Loyiha xarajatlari ro'yxati (filtr va sahifalash bilan).
    /// Список расходов по проекту (с фильтром и пагинацией).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll(
        Guid projectId,
        [FromQuery] string? category,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, notFound, forbidden) = await getAll.HandleAsync(
            new(projectId, category, from, to, page, size), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha xarajatni olish.
    /// Получить расход по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid projectId, Guid id, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await getById.HandleAsync(new(projectId, id), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Yangi xarajat qo'shish. Admin yoki Manager uchun.
    /// Добавить новый расход. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create(
        Guid projectId, [FromBody] CreateExpenseRequest req, CancellationToken ct)
    {
        var (data, notFound, forbidden, cashAccountNotFound, insufficientBalance) = await create.HandleAsync(
            new(projectId, req.Date, req.Category, req.Amount,
                req.Description, req.PaymentMethod, req.Note, req.CashAccountId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        if (insufficientBalance) return BadRequest(new { message = "Kassada mablag' yetarli emas. / Недостаточно средств на счёте." });
        return StatusCode(201, data);
    }

    /// <summary>
    /// Xarajatni yangilash. Admin yoki Manager uchun.
    /// Обновить расход. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid projectId, Guid id, [FromBody] UpdateExpenseRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, cashAccountNotFound, insufficientBalance) = await update.HandleAsync(
            new(projectId, id, req.Date, req.Category, req.Amount,
                req.Description, req.PaymentMethod, req.Note, req.CashAccountId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        if (insufficientBalance) return BadRequest(new { message = "Kassada mablag' yetarli emas. / Недостаточно средств на счёте." });
        return NoContent();
    }

    /// <summary>
    /// Xarajatni o'chirish. Admin yoki Manager uchun.
    /// Удалить расход. Для Admin или Manager.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken ct)
    {
        var (notFound, forbidden) = await delete.HandleAsync(new(projectId, id), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }
}
