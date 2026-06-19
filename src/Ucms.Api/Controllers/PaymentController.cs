namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Payments.Commands;
using Ucms.Application.Features.Payments.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Loyiha to'lovlarini boshqarish (mijoz va brigada to'lovlari).
/// Управление платежами проекта (платежи клиента и бригады).
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/payments")]
[Tags("Payment")]
[Authorize]
public class PaymentController(
    GetClientPayments.Handler    getClientPayments,
    CreateClientPayment.Handler  createClientPayment,
    GetBrigadePayments.Handler   getBrigadePayments,
    CreateBrigadePayment.Handler createBrigadePayment,
    GetFinancialSummary.Handler  getFinancialSummary) : ControllerBase
{
    public record CreateClientPaymentRequest(
        Guid? ActId, DateTimeOffset Date, decimal Amount,
        PaymentMethod PaymentMethod, string? Note, Guid CashAccountId);

    public record CreateProjectBrigadePaymentRequest(
        Guid BrigadeId, DateTimeOffset Date, decimal Amount,
        PaymentMethod PaymentMethod, Guid[] WorkLogIds, string? Note, Guid CashAccountId);

    /// <summary>
    /// Loyiha bo'yicha mijoz to'lovlari ro'yxati.
    /// Список платежей клиента по проекту.
    /// </summary>
    [HttpGet("client")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetClientPayments(
        Guid projectId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct = default)
    {
        var (data, notFound, forbidden) = await getClientPayments.HandleAsync(new(projectId, from, to), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Yangi mijoz to'lovi yaratish. Admin, Manager yoki Accountant uchun.
    /// Создать новый платёж клиента. Для Admin, Manager или Accountant.
    /// </summary>
    [HttpPost("client")]
    [Authorize(Roles = "Admin,Manager,Accountant")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateClientPayment(
        Guid projectId, [FromBody] CreateClientPaymentRequest req, CancellationToken ct)
    {
        var (data, notFound, forbidden, cashAccountNotFound) = await createClientPayment.HandleAsync(
            new(projectId, req.ActId, req.Date, req.Amount, req.PaymentMethod, req.Note, req.CashAccountId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        return StatusCode(201, data);
    }

    /// <summary>
    /// Loyiha bo'yicha brigada to'lovlari ro'yxati.
    /// Список платежей бригады по проекту.
    /// </summary>
    [HttpGet("brigade")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBrigadePayments(
        Guid projectId,
        [FromQuery] Guid? brigadeId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct = default)
    {
        var (data, notFound, forbidden) = await getBrigadePayments.HandleAsync(
            new(projectId, brigadeId, from, to), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Yangi brigada to'lovi yaratish. Admin, Manager yoki Accountant uchun.
    /// Создать новый платёж бригады. Для Admin, Manager или Accountant.
    /// </summary>
    [HttpPost("brigade")]
    [Authorize(Roles = "Admin,Manager,Accountant")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateBrigadePayment(
        Guid projectId, [FromBody] CreateProjectBrigadePaymentRequest req, CancellationToken ct)
    {
        var (data, notFound, forbidden, cashAccountNotFound, insufficientBalance) = await createBrigadePayment.HandleAsync(
            new(projectId, req.BrigadeId, req.Date, req.Amount, req.PaymentMethod, req.WorkLogIds, req.Note, req.CashAccountId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        if (insufficientBalance) return BadRequest(new { message = "Kassada mablag' yetarli emas. / Недостаточно средств на счёте." });
        return StatusCode(201, data);
    }

    /// <summary>
    /// Loyiha moliyaviy xulosasi (kirim-chiqim balansi).
    /// Финансовая сводка проекта (баланс доходов и расходов).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFinancialSummary(Guid projectId, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await getFinancialSummary.HandleAsync(new(projectId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }
}
