namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Features.StockDemands.Commands;
using Ucms.Application.Features.StockDemands.DTOs;
using Ucms.Application.Features.StockDemands.Queries;
using Ucms.Domain.Enums;

[Route("api/stock-demand")]
[ApiController]
[Authorize]
[Authorize(Policy = "warehouse.view")]
public class StockDemandController(
    GetStockDemands.Handler getAll,
    GetStockDemandById.Handler getById,
    FindStockDemand.Handler findByName,
    FindStockDemands.Handler findMany,
    GetRequestedDemands.Handler getRequested,
    GetReceivedDemands.Handler getReceived,
    CreateStockDemand.Handler create,
    UpdateStockDemand.Handler update,
    UpdateStockDemandStatus.Handler updateStatus,
    UpdateStockDemandBroadcastStatus.Handler updateBroadcastStatus) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<StockDemandModel>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            return Ok(await getAll.HandleAsync(new(), ct));
        }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StockDemandModel), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("name/{name}")]
    public async Task<IActionResult> FindByName(string name, CancellationToken ct = default)
        {
            return Ok(await findByName.HandleAsync(new(name), ct));
        }

    [HttpGet("search/{query}")]
    public async Task<IActionResult> Search(string query, CancellationToken ct = default)
        {
            return Ok(await findMany.HandleAsync(new(query), ct));
        }

    public record DemandsRequest(PagedRequest Filter, DateTime? From, DateTime? To, string? Name);

    [HttpPost("requested-demands")]
    [ProducesResponseType(typeof(PagedResult<RequestedDemandModel>), 200)]
    public async Task<IActionResult> GetRequestedDemands([FromBody] DemandsRequest req, CancellationToken ct = default)
    {
        return Ok(await getRequested.HandleAsync(new(req.Filter, req.From, req.To, req.Name), ct));
    }

    [HttpPost("received-demands")]
    [ProducesResponseType(typeof(PagedResult<ReceivedDemandModel>), 200)]
    public async Task<IActionResult> GetReceivedDemands([FromBody] DemandsRequest req, CancellationToken ct = default)
        {
            return Ok(await getReceived.HandleAsync(new(req.Filter, req.From, req.To, req.Name), ct));
        }

    public record CreateStockDemandRequest(string Name, string? Note, DateTimeOffset DemandDate,
        StockDemandStatus DemandStatus, Guid SenderId, Guid RecipientId, IEnumerable<StockDemandItemModel> Items);

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    public async Task<IActionResult> Create([FromBody] CreateStockDemandRequest req, CancellationToken ct = default)
    {
        var (id, error) = await create.HandleAsync(
            new(req.Name, req.Note, req.DemandDate, req.DemandStatus, req.SenderId, req.RecipientId, req.Items), ct);
        return error is not null ? Conflict(error) : Ok(id);
    }

    public record UpdateStockDemandRequest(Guid Id, string Name, string? Note, DateTimeOffset DemandDate,
        StockDemandStatus DemandStatus, Guid SenderId, Guid RecipientId, IEnumerable<StockDemandItemModel> Items);

    [HttpPut]
    [ProducesResponseType(typeof(Guid), 202)]
    public async Task<IActionResult> Update([FromBody] UpdateStockDemandRequest req, CancellationToken ct = default)
    {
        var (notFound, error) = await update.HandleAsync(
            new(req.Id, req.Name, req.Note, req.DemandDate, req.DemandStatus, req.SenderId, req.RecipientId, req.Items), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : Ok(req.Id);
    }

    public record UpdateDemandStatusRequest(Guid Id, StockDemandStatus Status);

    [HttpPut("update-status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateDemandStatusRequest req, CancellationToken ct = default)
    {
        var (notFound, error) = await updateStatus.HandleAsync(new(req.Id, req.Status), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : Ok(req.Id);
    }

    public record UpdateBroadcastStatusRequest(Guid Id, Guid OutcomeId, StockDemandBroadcastStatus Status);

    [HttpPut("update-broadcast-status")]
    public async Task<IActionResult> UpdateBroadcastStatus([FromBody] UpdateBroadcastStatusRequest req, CancellationToken ct = default)
    {
        var (notFound, error) = await updateBroadcastStatus.HandleAsync(new(req.Id, req.OutcomeId, req.Status), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : Ok(req.Id);
    }
}
