namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Features.StockSkus.DTOs;
using Ucms.Application.Features.StockSkus.Queries;

[Route("api/stock-sku")]
[ApiController]
[Authorize]
public class StockSkuController(
    GetStockSkus.Handler getStockSkus,
    GetCaseSkus.Handler getCaseSkus,
    GetStockInventory.Handler getInventory,
    GetFilteredStockSkus.Handler getFiltered,
    CheckSkuAmount.Handler checkAmount,
    GetProductBalance.Handler getBalance,
    GetStockSkuStats.Handler getStats) : ControllerBase
{
    public record GetStockSkusRequest(Guid OrganizationId, PagedRequest Filter,
        Guid? StockId, Guid? MeasurementUnitId, Guid? ProductId, Guid? ManufacturerId, string? Seria);

    [HttpPost]
    [ProducesResponseType(typeof(PagedResult<StockSkuModel>), 200)]
    public async Task<IActionResult> GetStockSkus([FromBody] GetStockSkusRequest req, CancellationToken ct = default)
        {
            return Ok(await getStockSkus.HandleAsync( new(req.OrganizationId, req.Filter, req.StockId, req.MeasurementUnitId, req.ProductId, req.ManufacturerId, req.Seria), ct));
        }

    public record GetCaseSkusRequest(PagedRequest Filter, Guid OrganizationId, Guid StockId, string? Seria);

    [HttpPost("case-sku")]
    [ProducesResponseType(typeof(PagedResult<StockSkuModel>), 200)]
    public async Task<IActionResult> GetCaseSkus([FromBody] GetCaseSkusRequest req, CancellationToken ct = default)
    {
        var (result, error) = await getCaseSkus.HandleAsync(new(req.Filter, req.OrganizationId, req.StockId, req.Seria), ct);
        return error is not null ? BadRequest(error) : Ok(result);
    }

    public record GetStockInventoryRequest(PagedRequest Filter, Guid? StockId, Guid? ProductId, Guid? OrganizationId);

    [HttpPost("inventory")]
    [ProducesResponseType(typeof(PagedResult<StockInventoryModel>), 200)]
    public async Task<IActionResult> GetStockInventory([FromBody] GetStockInventoryRequest req, CancellationToken ct = default)
        {
            return Ok(await getInventory.HandleAsync(new(req.Filter, req.StockId, req.ProductId, req.OrganizationId), ct));
        }

    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<StockSkuModel>), 200)]
    public async Task<IActionResult> SearchStockSkus([FromBody] PagedRequest filter, CancellationToken ct = default)
        {
            return Ok(await getFiltered.HandleAsync(new(filter), ct));
        }

    [HttpGet("check-amount")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> CheckAmount(
        [FromQuery] Guid skuId, [FromQuery] Guid stockId,
        [FromQuery] Guid measurementUnitId, [FromQuery] decimal amount, CancellationToken ct = default)
        {
            return Ok(await checkAmount.HandleAsync(new(skuId, stockId, measurementUnitId, amount), ct));
        }

    [HttpGet("product-balance")]
    [ProducesResponseType(typeof(decimal), 200)]
    public async Task<IActionResult> GetProductBalance(
        [FromQuery] Guid stockId, [FromQuery] Guid productId, [FromQuery] Guid measurementUnitId, CancellationToken ct = default)
        {
            return Ok(await getBalance.HandleAsync(new(stockId, productId, measurementUnitId), ct));
        }

    [HttpGet("stats/{organizationId:guid}")]
    [ProducesResponseType(typeof(StockSkuStatModel), 200)]
    public async Task<IActionResult> GetStats(Guid organizationId, CancellationToken ct = default)
        {
            return Ok(await getStats.HandleAsync(new(organizationId), ct));
        }
}
