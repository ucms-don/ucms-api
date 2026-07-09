namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Features.Stocks.Commands;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Application.Features.Stocks.Queries;
using Ucms.Application.Services;
using Ucms.Domain.Enums;

[Route("api/stock")]
[ApiController]
[Authorize]
[Authorize(Policy = "warehouse.view")]
public class StockController(
    GetStocks.Handler getAll,
    GetFilteredStocks.Handler getFiltered,
    GetAllCases.Handler getCases,
    GetCentralStocks.Handler getCentral,
    GetStockById.Handler getById,
    FindStockByCode.Handler findByCode,
    FindStocks.Handler findStocks,
    CreateStock.Handler create,
    UpdateStock.Handler update,
    DeleteStock.Handler delete,
    DeleteStocks.Handler deleteRange,
    IStockCodeGenerator codeGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<StockModel>), 200)]
    public async Task<IActionResult> GetStocks(
        [FromQuery] Guid organizationId, [FromQuery] bool? unattached,
        [FromQuery] StockType? stockType, [FromQuery] StockCategory? stockCategory,
        [FromQuery] string? query, [FromQuery] bool? includeChild, CancellationToken ct = default)
        {
            return Ok(await getAll.HandleAsync(new(organizationId, unattached, stockType, stockCategory, query, includeChild), ct));
        }

    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<StockModel>), 200)]
    public async Task<IActionResult> SearchStocks([FromBody] PagedRequest filter, CancellationToken ct = default)
        {
            return Ok(await getFiltered.HandleAsync(new(filter), ct));
        }

    [HttpGet("cases")]
    [ProducesResponseType(typeof(List<StockModel>), 200)]
    public async Task<IActionResult> GetCases(CancellationToken ct = default)
        {
            return Ok(await getCases.HandleAsync(new(), ct));
        }

    [HttpGet("central-stock/{organizationId:guid}")]
    [ProducesResponseType(typeof(List<StockModel>), 200)]
    public async Task<IActionResult> GetCentralStocks(Guid organizationId, CancellationToken ct = default)
        {
            return Ok(await getCentral.HandleAsync(new(organizationId), ct));
        }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StockModel), 200)]
    public async Task<IActionResult> GetStock(Guid id, CancellationToken ct = default)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> FindByCode(string code, CancellationToken ct = default)
        {
            return Ok(await findByCode.HandleAsync(new(code), ct));
        }

    [HttpGet("search/{query}")]
    public async Task<IActionResult> FindStocks(string query, CancellationToken ct = default)
        {
            return Ok(await findStocks.HandleAsync(new(query), ct));
        }

    [HttpGet("generate-code")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> GenerateCode(CancellationToken ct = default)
        {
            var code = await codeGenerator.GenerateAsync(ct);
            return Ok(code);
        }

    public record CreateStockRequest(string Name, string NameRu, string? NameEn, string? NameKa,
        string Code, StorageCondition StorageCondition, StockType StockType,
        StockCategory StockCategory, Guid OrganizationId, Guid? ParentId, Guid[] EmployeeIds);

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    public async Task<IActionResult> CreateStock([FromBody] CreateStockRequest req, CancellationToken ct = default)
    {
        var (id, error) = await create.HandleAsync(
            new(req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code,
                req.StorageCondition, req.StockType, req.StockCategory,
                req.OrganizationId, req.ParentId, req.EmployeeIds), ct);
        return error is not null ? Conflict(error) : Ok(id);
    }

    public record UpdateStockRequest(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, StorageCondition StorageCondition, StockType StockType,
        StockCategory StockCategory, Guid OrganizationId, Guid? ParentId, Guid[] EmployeeIds);

    [HttpPut]
    [ProducesResponseType(typeof(Guid), 202)]
    public async Task<IActionResult> UpdateStock([FromBody] UpdateStockRequest req, CancellationToken ct = default)
    {
        var (notFound, error) = await update.HandleAsync(
            new(req.Id, req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code,
                req.StorageCondition, req.StockType, req.StockCategory,
                req.OrganizationId, req.ParentId, req.EmployeeIds), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : Ok(req.Id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStock(Guid id, CancellationToken ct = default)
    {
        var (notFound, error) = await delete.HandleAsync(new(id), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : NoContent();
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteStocks([FromBody] Guid[] ids, CancellationToken ct = default)
    {
        var (_, error) = await deleteRange.HandleAsync(new(ids), ct);
        return error is not null ? Conflict(error) : NoContent();
    }
}
