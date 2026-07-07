namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Abstractions.Storage;
using Ucms.Application.Features.Incomes.Commands;
using Ucms.Application.Features.Incomes.DTOs;
using Ucms.Application.Features.Incomes.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Kirim operatsiyalarini boshqarish.
/// Управление операциями прихода.
/// </summary>
[Route("api/income")]
[ApiController]
[Authorize]
[Authorize(Policy = "finance.view")]
public class IncomeController(
    GetIncomeById.Handler      getById,
    FindIncome.Handler         findByName,
    FindIncomes.Handler        findMany,
    GetFilteredIncomes.Handler getFiltered,
    CreateIncome.Handler       create,
    UpdateIncome.Handler       update,
    UpdateIncomeStatus.Handler updateStatus,
    DeleteIncome.Handler       delete,
    UploadIncomeFile.Handler   upload) : ControllerBase
{
    /// <summary>
    /// ID bo'yicha kirimni olish.
    /// Получить приход по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IncomeModel), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Nom bo'yicha kirimni qidirish.
    /// Найти приход по наименованию.
    /// </summary>
    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(IncomeModel), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> FindByName(string name, CancellationToken ct = default)
    {
        var result = await findByName.HandleAsync(new(name), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Matn bo'yicha kirimlarni qidirish.
    /// Поиск приходов по тексту.
    /// </summary>
    [HttpGet("search/{query}")]
    [ProducesResponseType(typeof(List<IncomeModel>), 200)]
    public async Task<IActionResult> Search(string query, CancellationToken ct = default)
    {
        return Ok(await findMany.HandleAsync(new(query), ct));
    }

    public record GetIncomesRequest(
        PagedRequest Filter,
        Guid?        StockId,
        string?      Query,
        DateTime?    From,
        DateTime?    To);

    /// <summary>
    /// Filtrlangan kirimlar jadval ro'yxati.
    /// Фильтрованный табличный список приходов.
    /// </summary>
    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<IncomeModel>), 200)]
    public async Task<IActionResult> GetFiltered([FromBody] GetIncomesRequest req, CancellationToken ct = default)
    {
        return Ok(await getFiltered.HandleAsync(new(req.Filter, req.StockId, req.Query, req.From, req.To), ct));
    }

    public record CreateIncomeRequest(
        string                             Name,
        string?                            Note,
        IncomeType                         IncomeType,
        IncomeStatus                       IncomeStatus,
        PaymentType                        PaymentType,
        DateTimeOffset                     IncomeDate,
        Guid                               StockId,
        IEnumerable<CreateIncomeItemModel> IncomeItems);

    /// <summary>
    /// Yangi kirim yaratish.
    /// Создать новый приход.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    public async Task<IActionResult> Create([FromBody] CreateIncomeRequest req, CancellationToken ct = default)
    {
        var id = await create.HandleAsync(
            new(req.Name, req.Note, req.IncomeType, req.IncomeStatus, req.PaymentType,
                req.IncomeDate, req.StockId, req.IncomeItems), ct);
        return StatusCode(201, id);
    }

    public record UpdateIncomeRequest(
        Guid                               Id,
        string                             Name,
        string?                            Note,
        IncomeType                         IncomeType,
        IncomeStatus                       IncomeStatus,
        PaymentType                        PaymentType,
        DateTimeOffset                     IncomeDate,
        Guid                               StockId,
        IEnumerable<CreateIncomeItemModel> IncomeItems);

    /// <summary>
    /// Kirim ma'lumotlarini yangilash.
    /// Обновить данные прихода.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update([FromBody] UpdateIncomeRequest req, CancellationToken ct = default)
    {
        var ok = await update.HandleAsync(
            new(req.Id, req.Name, req.Note, req.IncomeType, req.IncomeStatus, req.PaymentType,
                req.IncomeDate, req.StockId, req.IncomeItems), ct);
        return ok ? NoContent() : NotFound();
    }

    public record UpdateStatusRequest(Guid Id, IncomeStatus Status);

    /// <summary>
    /// Kirim holatini yangilash.
    /// Обновить статус прихода.
    /// </summary>
    [HttpPut("update-status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest req, CancellationToken ct = default)
    {
        var ok = await updateStatus.HandleAsync(new(req.Id, req.Status), ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Kirimni o'chirish.
    /// Удалить приход.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var ok = await delete.HandleAsync(new(id), ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Kirimga fayl yuklash (maks. 10 MB, faqat PDF).
    /// Загрузить файл к приходу (макс. 10 МБ, только PDF).
    /// </summary>
    [HttpPost("upload/{id:guid}")]
    [RequestSizeLimit(10L * 1024L * 1024L)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L)]
    [ProducesResponseType(typeof(FileEntryModel), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Upload(Guid id, IFormFile file, CancellationToken ct = default)
    {
        var (result, error) = await upload.HandleAsync(new(id, file), ct);
        return error is not null ? BadRequest(new { message = error }) : Ok(result);
    }

}
