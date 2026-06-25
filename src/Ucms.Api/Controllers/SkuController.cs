namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Features.Skus.Commands;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Features.Skus.Queries;
using Ucms.Domain.Enums;

[Route("api/sku")]
[ApiController]
[Authorize]
public class SkuController(
    GetSkus.Handler getAll,
    GetFilteredSkus.Handler getFiltered,
    GetSkuById.Handler getById,
    GetProductSkus.Handler getProductSkus,
    GetProductStockSkus.Handler getProductStockSkus,
    FindSkuBySerial.Handler findBySerial,
    FindSkus.Handler findSkus,
    CheckSkuForUsed.Handler checkUsed,
    CreateSku.Handler create,
    UpdateSku.Handler update,
    DeleteSku.Handler delete,
    DeleteSkus.Handler deleteRange) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<SkuModel>), 200)]
    public async Task<IActionResult> GetSkus(CancellationToken ct = default)
        {
            return Ok(await getAll.HandleAsync(new(), ct));
        }

    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<SkuModel>), 200)]
    public async Task<IActionResult> SearchSkus([FromBody] GetSkusRequest req, CancellationToken ct = default)
        {
            return Ok(await getFiltered.HandleAsync(new(req, req.Query, req.Seria), ct));
        }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SkuModel), 200)]
    public async Task<IActionResult> GetSku(Guid id, CancellationToken ct = default)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("product/{id:guid}")]
    [ProducesResponseType(typeof(List<SkuModel>), 200)]
    public async Task<IActionResult> GetProductSkus(Guid id, CancellationToken ct = default)
        {
            return Ok(await getProductSkus.HandleAsync(new(id), ct));
        }

    [HttpGet("product-stock")]
    [ProducesResponseType(typeof(List<SkuModel>), 200)]
    public async Task<IActionResult> GetProductStockSkus(
        [FromQuery] Guid? productId, [FromQuery] Guid? stockId,
        [FromQuery] List<ProductType>? types, [FromQuery] string? query,
        [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            return Ok(await getProductStockSkus.HandleAsync( new(new PagedRequest { Page = page, PageSize = size }, productId, stockId, types, query), ct));
        }

    [HttpGet("serial/{serial}")]
    [ProducesResponseType(typeof(SkuModel), 200)]
    public async Task<IActionResult> FindBySerial(string serial, CancellationToken ct = default)
        {
            return Ok(await findBySerial.HandleAsync(new(serial), ct));
        }

    [HttpGet("search/{query}")]
    [ProducesResponseType(typeof(List<SkuModel>), 200)]
    public async Task<IActionResult> SearchByQuery(string query, CancellationToken ct = default)
        {
            return Ok(await findSkus.HandleAsync(new(query), ct));
        }

    [HttpGet("check-for-used/{id:guid}")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> CheckForUsed(Guid id, CancellationToken ct = default)
        {
            return Ok(await checkUsed.HandleAsync(new(id), ct));
        }

    public record CreateSkuRequest(string Name, string NameRu, string? NameEn, string? NameKa,
        string SerialNumber, Guid ProductId, Guid? ManufacturerId, Guid MeasurementUnitId,
        Guid? SupplierId, decimal Price, decimal Amount, DateTimeOffset ExpirationDate, SkuStatus Status,
        Guid? CashAccountId, Guid? StockId);

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    public async Task<IActionResult> CreateSku([FromBody] CreateSkuRequest req, CancellationToken ct = default)
    {
        var (id, error) = await create.HandleAsync(
            new(req.Name, req.NameRu, req.NameEn, req.NameKa, req.SerialNumber,
                req.ProductId, req.ManufacturerId, req.MeasurementUnitId, req.SupplierId,
                req.Price, req.Amount, req.ExpirationDate, req.Status, req.CashAccountId, req.StockId), ct);
        return error is not null ? BadRequest(new { message = error }) : Ok(id);
    }

    public record UpdateSkuRequest(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        string SerialNumber, Guid ProductId, Guid? ManufacturerId, Guid MeasurementUnitId,
        Guid? SupplierId, decimal Price, decimal Amount, DateTimeOffset ExpirationDate, SkuStatus Status,
        Guid? CashAccountId);

    [HttpPut]
    [ProducesResponseType(typeof(Guid), 202)]
    public async Task<IActionResult> UpdateSku([FromBody] UpdateSkuRequest req, CancellationToken ct = default)
    {
        var (found, error) = await update.HandleAsync(
            new(req.Id, req.Name, req.NameRu, req.NameEn, req.NameKa, req.SerialNumber,
                req.ProductId, req.ManufacturerId, req.MeasurementUnitId, req.SupplierId,
                req.Price, req.Amount, req.ExpirationDate, req.Status, req.CashAccountId), ct);
        if (!found) return NotFound();
        return error is not null ? BadRequest(new { message = error }) : Ok(req.Id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSku(Guid id, CancellationToken ct = default)
    {
        var (notFound, error) = await delete.HandleAsync(new(id), ct);
        if (notFound) return NotFound();
        return error is not null ? BadRequest(new { message = error }) : NoContent();
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteSkus([FromBody] Guid[] ids, CancellationToken ct = default)
    {
        var (_, error) = await deleteRange.HandleAsync(new(ids), ct);
        return error is not null ? BadRequest(new { message = error }) : NoContent();
    }
}

// Inline request type for table-list
public class GetSkusRequest : PagedRequest
{
    public string? Query { get; init; }
    public string? Seria { get; init; }
}
