namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Abstractions.Storage;
using Ucms.Application.Features.Outcomes.Commands;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Application.Features.Outcomes.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Chiqim operatsiyalarini boshqarish.
/// Управление операциями расхода.
/// </summary>
[Route("api/outcome")]
[ApiController]
[Authorize]
public class OutcomeController(
    GetOutcomeById.Handler getById,
    GetOutcomeByExecutionId.Handler getByExecutionId,
    FindOutcome.Handler findOutcome,
    FindOutcomes.Handler findOutcomes,
    GetFilteredOutcomes.Handler getFiltered,
    GetOutcomeStats.Handler getStats,
    CreateOutcome.Handler create,
    UpdateOutcome.Handler update,
    UpdateOutcomeStatus.Handler updateStatus,
    DeleteOutcome.Handler delete,
    UploadOutcomeFile.Handler uploadFile) : ControllerBase
{
    public record GetOutcomesRequest(PagedRequest Filter, Guid? StockId, string? Query, DateTime? From, DateTime? To);
    public record GetOutcomeStatsRequest(Guid OrganizationId, DateTime From, DateTime To, DateTime PreviousFrom, DateTime PreviousTo);
    public record UpdateOutcomeStatusRequest(Guid Id, OutcomeStatus Status);
    public record CreateOutcomeRequest(string Name, string? Note, OutcomeType OutcomeType, OutcomeStatus OutcomeStatus,
        PaymentType PaymentType, DateTimeOffset OutcomeDate, Guid StockId, Guid? IncomeStockId,
        Guid? ExecutionId, IEnumerable<CreateOutcomeItemModel> OutcomeItems);
    public record UpdateOutcomeRequest(Guid Id, string Name, string? Note, OutcomeType OutcomeType, OutcomeStatus OutcomeStatus,
        PaymentType PaymentType, DateTimeOffset OutcomeDate, Guid StockId, Guid? IncomeStockId,
        Guid? ExecutionId, IEnumerable<CreateOutcomeItemModel> OutcomeItems);

    /// <summary>
    /// ID bo'yicha chiqimni olish.
    /// Получить расход по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OutcomeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOutcome(Guid id, CancellationToken ct)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Bajarish ID bo'yicha chiqimni olish.
    /// Получить расход по ID выполнения.
    /// </summary>
    [HttpGet("execution/{executionId:guid}")]
    [ProducesResponseType(typeof(OutcomeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionOutcome(Guid executionId, CancellationToken ct)
    {
        var result = await getByExecutionId.HandleAsync(new(executionId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Filtrlangan chiqimlar jadval ro'yxati.
    /// Фильтрованный табличный список расходов.
    /// </summary>
    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<OutcomeModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilteredOutcomes([FromBody] GetOutcomesRequest request, CancellationToken ct)
    {
        return Ok(await getFiltered.HandleAsync(new(request.Filter, request.StockId, request.Query, request.From, request.To), ct));
    }

    /// <summary>
    /// Nom bo'yicha chiqimni qidirish.
    /// Найти расход по наименованию.
    /// </summary>
    [HttpGet("name")]
    [ProducesResponseType(typeof(OutcomeModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutcomeByName([FromQuery] string name, CancellationToken ct)
    {
        return Ok(await findOutcome.HandleAsync(new(name), ct));
    }

    /// <summary>
    /// Matn bo'yicha chiqimlarni qidirish.
    /// Поиск расходов по тексту.
    /// </summary>
    [HttpGet("search/{query}")]
    [ProducesResponseType(typeof(OutcomeModel[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchOutcomes(string query, CancellationToken ct)
    {
        return Ok(await findOutcomes.HandleAsync(new(query), ct));
    }

    /// <summary>
    /// Yangi chiqim yaratish.
    /// Создать новый расход.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOutcome([FromBody] CreateOutcomeRequest request, CancellationToken ct)
    {
        var id = await create.HandleAsync(new(request.Name, request.Note, request.OutcomeType, request.OutcomeStatus,
            request.PaymentType, request.OutcomeDate, request.StockId, request.IncomeStockId, request.ExecutionId,
            request.OutcomeItems), ct);
        return StatusCode(201, id);
    }

    /// <summary>
    /// Chiqim ma'lumotlarini yangilash.
    /// Обновить данные расхода.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOutcome([FromBody] UpdateOutcomeRequest request, CancellationToken ct)
    {
        var found = await update.HandleAsync(new(request.Id, request.Name, request.Note, request.OutcomeType,
            request.OutcomeStatus, request.PaymentType, request.OutcomeDate, request.StockId, request.IncomeStockId,
            request.ExecutionId, request.OutcomeItems), ct);
        return found ? NoContent() : NotFound();
    }

    /// <summary>
    /// Chiqim holatini yangilash.
    /// Обновить статус расхода.
    /// </summary>
    [HttpPut("update-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutcomeStatus([FromBody] UpdateOutcomeStatusRequest req, CancellationToken ct)
    {
        var (notFound, error) = await updateStatus.HandleAsync(new(req.Id, req.Status), ct);
        if (notFound)          return NotFound();
        if (error is not null) return BadRequest(new { message = error });
        return NoContent();
    }

    /// <summary>
    /// Chiqimni o'chirish.
    /// Удалить расход.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOutcome(Guid id, CancellationToken ct)
    {
        var ok = await delete.HandleAsync(new(id), ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Chiqim statistikasi.
    /// Статистика расходов.
    /// </summary>
    [HttpPost("stats")]
    [ProducesResponseType(typeof(OutcomeStatsModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats([FromBody] GetOutcomeStatsRequest req, CancellationToken ct)
    {
        return Ok(await getStats.HandleAsync(
            new(req.OrganizationId, req.From, req.To, req.PreviousFrom, req.PreviousTo), ct));
    }

    /// <summary>
    /// Chiqimga fayl yuklash (maks. 10 MB, faqat PDF).
    /// Загрузить файл к расходу (макс. 10 МБ, только PDF).
    /// </summary>
    [HttpPost("upload/{id:guid}")]
    [RequestSizeLimit(10L * 1024L * 1024L)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L)]
    [ProducesResponseType(typeof(FileEntryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(Guid id, IFormFile file, CancellationToken ct)
    {
        var (result, error) = await uploadFile.HandleAsync(new(id, file), ct);
        return error is not null ? BadRequest(new { message = error }) : Ok(result);
    }

}
