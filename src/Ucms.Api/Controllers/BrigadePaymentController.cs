namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Payments.Commands;
using Ucms.Application.Features.Payments.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Barcha loyihalar bo'yicha brigada to'lovlari (global ro'yxat).
/// Глобальный список выплат бригадам по всем проектам.
/// </summary>
[ApiController]
[Route("api/brigade-payments")]
[Tags("BrigadePayment")]
[Authorize]
public class BrigadePaymentController(
    GetAllBrigadePayments.Handler getAll,
    CreateBrigadePayment.Handler  create) : ControllerBase
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
        var (data, projectNotFound, forbidden, cashNotFound) = await create.HandleAsync(
            new(req.ProjectId, req.BrigadeId, req.Date, req.Amount,
                req.PaymentMethod, req.WorkLogIds ?? [], req.Note, req.CashAccountId), ct);

        if (projectNotFound) return NotFound(new { message = "Loyiha topilmadi" });
        if (forbidden)       return Forbid();
        if (cashNotFound)    return BadRequest(new { message = "Kassa hisobi topilmadi" });
        return StatusCode(201, data);
    }
}
