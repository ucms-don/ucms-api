namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Features.Suppliers.Commands;
using Ucms.Application.Features.Suppliers.DTOs;
using Ucms.Application.Features.Suppliers.Queries;

[Route("api/suppliers")]
[ApiController]
[Authorize]
[Authorize(Policy = "refs.view")]
public class SupplierController(
    GetFilteredSuppliers.Handler getFiltered,
    GetSupplierById.Handler getById,
    FindSupplierByCode.Handler findByCode,
    CreateSupplier.Handler create,
    UpdateSupplier.Handler update,
    DeleteSupplier.Handler delete,
    DeleteSuppliers.Handler deleteRange) : ControllerBase
{
   
    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<SupplierModel>), 200)]
    public async Task<IActionResult> SearchSuppliers([FromBody] PagedRequest filter, CancellationToken ct = default)
        {
            return Ok(await getFiltered.HandleAsync(new(filter), ct));
        }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupplierModel), 200)]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken ct = default)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> FindByCode(string code, CancellationToken ct = default)
        {
            return Ok(await findByCode.HandleAsync(new(code), ct));
        }

    public record CreateSupplierRequest(string Name, string NameRu, string? NameEn, string? NameKa, string Code);

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest req, CancellationToken ct = default)
    {
        var (id, error) = await create.HandleAsync(new(req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code), ct);
        return error is not null ? Conflict(error) : Ok(id);
    }

    public record UpdateSupplierRequest(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa, string Code);

    [HttpPut]
    [ProducesResponseType(typeof(Guid), 202)]
    public async Task<IActionResult> UpdateSupplier([FromBody] UpdateSupplierRequest req, CancellationToken ct = default)
    {
        var ok = await update.HandleAsync(new(req.Id, req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code), ct);
        return ok ? Ok(req.Id) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSupplier(Guid id, CancellationToken ct = default)
    {
        var (notFound, error) = await delete.HandleAsync(new(id), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : NoContent();
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteSuppliers([FromBody] Guid[] ids, CancellationToken ct = default)
    {
        var (_, error) = await deleteRange.HandleAsync(new(ids), ct);
        return error is not null ? Conflict(error) : NoContent();
    }
}
