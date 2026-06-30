namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.CashTransactions.Commands;
using Ucms.Application.Features.CashTransactions.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Kassa pul harakatlarini boshqarish (yetkazib beruvchi to'lovi, kredit, investitsiya va h.k.).
/// Управление операциями движения денежных средств (оплата поставщику, кредит, инвестиции и т.д.).
/// </summary>
[ApiController]
[Route("api/cash-transactions")]
[Tags("CashTransaction")]
[Authorize]
public class CashTransactionController(
    GetCashTransactions.Handler    getAll,
    GetCashTransactionById.Handler getById,
    GetPartnerBalances.Handler     getPartnerBalances,
    CreateCashTransaction.Handler  create,
    UpdateCashTransaction.Handler  update,
    DeleteCashTransaction.Handler  delete) : ControllerBase
{
    public record CreateCashTransactionRequest(
        Guid CashAccountId, CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? Note);

    public record UpdateCashTransactionRequest(
        Guid CashAccountId, CashDirection Direction, CashTransactionType TransactionType,
        FinancePartnerType PartnerType, Guid? PartnerId, string? PartnerName, decimal Amount, DateTimeOffset Date,
        Guid? ProjectId, string? Note);

    /// <summary>
    /// Pul harakatlari ro'yxati (kassa, partner, loyiha, yo'nalish, tur va sana filtri bilan, sahifalash bilan).
    /// Список денежных операций (с фильтром по кассе, партнёру, проекту, направлению, типу и дате, с пагинацией).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? cashAccountId,
        [FromQuery] FinancePartnerType? partnerType,
        [FromQuery] Guid? partnerId,
        [FromQuery] Guid? projectId,
        [FromQuery] CashDirection? direction,
        [FromQuery] CashTransactionType? transactionType,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(
            new(cashAccountId, partnerType, partnerId, projectId, direction, transactionType, dateFrom, dateTo, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Partnerlar (Supplier/Owner/Lender/Other) bo'yicha balans hisoboti.
    /// Отчёт по балансу партнёров (Supplier/Owner/Lender/Other).
    /// </summary>
    [HttpGet("partner-balances")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetPartnerBalances(
        [FromQuery] FinancePartnerType? partnerType,
        [FromQuery] Guid? partnerId,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getPartnerBalances.HandleAsync(new(partnerType, partnerId), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// ID bo'yicha pul harakatini olish.
    /// Получить операцию по ID.
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
    /// Yangi pul harakati qo'shish. Admin yoki Manager uchun.
    /// Добавить новую денежную операцию. Для Admin или Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateCashTransactionRequest req, CancellationToken ct)
    {
        var (result, cashAccountNotFound, projectNotFound, forbidden) = await create.HandleAsync(
            new(req.CashAccountId, req.Direction, req.TransactionType, req.PartnerType, req.PartnerId, req.PartnerName,
                req.Amount, req.Date, req.ProjectId, req.Note), ct);

        if (forbidden) return Forbid();
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        if (projectNotFound) return BadRequest(new { message = "Loyiha topilmadi. / Проект не найден." });
        if (result is null) return BadRequest(new { message = "Foydalanuvchiga tashkilot biriktirilmagan. / Пользователю не привязана организация." });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Pul harakati ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// Обновить данные денежной операции. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCashTransactionRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, cashAccountNotFound, projectNotFound) = await update.HandleAsync(
            new(id, req.CashAccountId, req.Direction, req.TransactionType, req.PartnerType, req.PartnerId, req.PartnerName,
                req.Amount, req.Date, req.ProjectId, req.Note), ct);

        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        if (cashAccountNotFound) return BadRequest(new { message = "Kassa/hisob topilmadi. / Касса/счёт не найден." });
        if (projectNotFound) return BadRequest(new { message = "Loyiha topilmadi. / Проект не найден." });
        return NoContent();
    }

    /// <summary>
    /// Pul harakatini o'chirish. Admin yoki Manager uchun.
    /// Удалить денежную операцию. Для Admin или Manager.
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
