namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.AccountTransfers.Commands;
using Ucms.Application.Features.AccountTransfers.Queries;

/// <summary>
/// Kassadan kassaga o'tkazmalar (bank → naqd va aksincha).
/// Переводы между кассами/счетами (банк → касса и наоборот).
/// </summary>
[ApiController]
[Route("api/account-transfers")]
[Tags("AccountTransfer")]
[Authorize]
public class AccountTransferController(
    GetAccountTransfers.Handler  getAll,
    CreateAccountTransfer.Handler create,
    UpdateAccountTransfer.Handler update,
    DeleteAccountTransfer.Handler delete) : ControllerBase
{
    public record CreateRequest(
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount,
        decimal Commission,
        string TransferredBy,
        DateTimeOffset Date,
        string? Note);

    public record UpdateRequest(
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount,
        decimal Commission,
        string TransferredBy,
        DateTimeOffset Date,
        string? Note);

    /// <summary>
    /// O'tkazmalar ro'yxati (filtr va sahifalash bilan).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? fromAccountId,
        [FromQuery] Guid? toAccountId,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(
            new(fromAccountId, toAccountId, dateFrom, dateTo, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }

    /// <summary>
    /// Yangi o'tkazma yaratish. Admin yoki Manager uchun.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateRequest req, CancellationToken ct)
    {
        var (result, fromNotFound, toNotFound, forbidden, error) = await create.HandleAsync(
            new(req.FromAccountId, req.ToAccountId, req.Amount, req.Commission, req.TransferredBy, req.Date, req.Note), ct);

        if (forbidden)     return Forbid();
        if (fromNotFound)  return BadRequest(new { message = "Manba kassa/hisob topilmadi" });
        if (toNotFound)    return BadRequest(new { message = "Maqsad kassa topilmadi" });
        if (error != null) return BadRequest(new { message = error });
        return StatusCode(201, result);
    }

    /// <summary>
    /// O'tkazma ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, fromNotFound, toNotFound, error) = await update.HandleAsync(
            new(id, req.FromAccountId, req.ToAccountId, req.Amount, req.Commission, req.TransferredBy, req.Date, req.Note), ct);

        if (notFound)      return NotFound();
        if (forbidden)     return Forbid();
        if (fromNotFound)  return BadRequest(new { message = "Manba kassa/hisob topilmadi" });
        if (toNotFound)    return BadRequest(new { message = "Maqsad kassa topilmadi" });
        if (error != null) return BadRequest(new { message = error });
        return NoContent();
    }

    /// <summary>
    /// O'tkazmani o'chirish. Admin yoki Manager uchun.
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
