namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Payments.Commands;
using Ucms.Application.Features.Payments.Queries;
using Ucms.Domain.Constants;
using Ucms.Domain.Enums;

/// <summary>
/// Barcha loyihalar bo'yicha brigada to'lovlari (global ro'yxat).
/// Глобальный список выплат бригадам по всем проектам.
/// </summary>
[ApiController]
[Route("api/brigade-payments")]
[Tags("BrigadePayment")]
[Authorize]
[Authorize(Policy = "brigades.view")]
public class BrigadePaymentController(
    GetAllBrigadePayments.Handler  getAll,
    CreateBrigadePayment.Handler   create,
    UpdateBrigadePayment.Handler   update,
    CancelBrigadePayment.Handler   cancel) : ControllerBase
{
    public record CreateBrigadePaymentRequest(
        Guid            ProjectId,
        Guid            BrigadeId,
        DateTimeOffset  Date,
        decimal         Amount,
        PaymentMethod   PaymentMethod,
        Guid[]?         WorkLogIds,
        string?         Note,
        Guid            CashAccountId);

    /// <summary>
    /// Barcha brigada to'lovlari ro'yxati (brigada, loyiha, sana filtrlari bilan).
    /// Список всех выплат бригадам (с фильтрами по бригаде, проекту и дате).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? brigadeId,
        [FromQuery] Guid? projectId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(new(brigadeId, projectId, from, to, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Yangi brigada to'lovi yaratish.
    /// Создать новую выплату бригаде.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBrigadePaymentRequest req, CancellationToken ct)
    {
        var (data, projectNotFound, forbidden, cashNotFound, insufficientBalance) = await create.HandleAsync(
            new(req.ProjectId, req.BrigadeId, req.Date, req.Amount,
                req.PaymentMethod, req.WorkLogIds ?? [], req.Note, req.CashAccountId), ct);

        if (projectNotFound) return NotFound(new { message = "Loyiha topilmadi. / Проект не найден." });
        if (forbidden)       return Forbid();
        if (cashNotFound)    return BadRequest(new { message = "Kassa hisobi topilmadi. / Кассовый счёт не найден." });
        if (insufficientBalance) return BadRequest(new { message = "Kassada mablag' yetarli emas. / Недостаточно средств на счёте." });
        return StatusCode(201, data);
    }

    public record UpdateBrigadePaymentRequest(
        DateTimeOffset Date,
        decimal        Amount,
        PaymentMethod  PaymentMethod,
        string?        Note);

    /// <summary>
    /// Brigada to'lovini yangilash.
    /// Обновить выплату бригаде.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrigadePaymentRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, insufficientBalance) = await update.HandleAsync(
            new(id, req.Date, req.Amount, req.PaymentMethod, req.Note), ct);

        if (notFound)           return NotFound(new { message = "To'lov topilmadi. / Выплата не найдена." });
        if (forbidden)          return Forbid();
        if (insufficientBalance) return BadRequest(new { message = "Kassada mablag' yetarli emas. / Недостаточно средств на счёте." });
        return NoContent();
    }

    /// <summary>
    /// Brigada to'lovini bekor qilish. Kassa balansi tiklanadi, WorkLoglar Confirmed holatga qaytadi.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.Finance.Cancel)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var (notFound, forbidden, alreadyCancelled) = await cancel.HandleAsync(new(id), ct);
        if (notFound)         return NotFound(new { message = "To'lov topilmadi." });
        if (forbidden)        return Forbid();
        if (alreadyCancelled) return Conflict(new { message = "To'lov allaqachon bekor qilingan." });
        return NoContent();
    }
}
