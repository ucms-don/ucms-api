namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryForge.Abstractions;
using QueryForge.Models;
using Ucms.Application.Features.Products.Commands;
using Ucms.Application.Features.Products.DTOs;
using Ucms.Application.Features.Products.Queries;
using Ucms.Domain.Enums;

[Route("api/products")]
[ApiController]
[Authorize]
[Authorize(Policy = "refs.view")]
public class ProductController(
    GetProducts.Handler getProducts,
    GetFilteredProducts.Handler getFiltered,
    GetProductById.Handler getById,
    FindProductByCode.Handler findByCode,
    FindProductByName.Handler findByName,
    CreateProduct.Handler create,
    UpdateProduct.Handler update,
    DeleteProduct.Handler delete,
    DeleteProducts.Handler deleteRange) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductModel>), 200)]
    public async Task<IActionResult> GetProducts([FromQuery] string? query, [FromQuery] List<ProductType>? type,
        [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            return Ok(await getProducts.HandleAsync(new(query, type, page, size), ct));
        }

    [HttpPost("table-list")]
    [ProducesResponseType(typeof(PagedResult<ProductModel>), 200)]
    public async Task<IActionResult> SearchProducts([FromBody] PagedRequest filter, CancellationToken ct = default)
        {
            return Ok(await getFiltered.HandleAsync(new(filter), ct));
        }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductModel), 200)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct = default)
    {
        var result = await getById.HandleAsync(new(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ProductModel), 200)]
    public async Task<IActionResult> GetProductByCode(string code, CancellationToken ct = default)
        {
            return Ok(await findByCode.HandleAsync(new(code), ct));
        }

    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(ProductModel), 200)]
    public async Task<IActionResult> GetProductByName(string name, CancellationToken ct = default)
        {
            return Ok(await findByName.HandleAsync(new(name), ct));
        }

    public record CreateProductRequest(string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, string? InternationalCode, string? InternationalName, string? AlternativeName, ProductType Type);

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest req, CancellationToken ct = default)
    {
        var (id, error) = await create.HandleAsync(
            new(req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code,
                req.InternationalCode, req.InternationalName, req.AlternativeName, req.Type), ct);
        return error is not null ? Conflict(error) : Ok(id);
    }

    public record UpdateProductRequest(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, string? InternationalCode, string? InternationalName, string? AlternativeName, ProductType Type);

    [HttpPut]
    [ProducesResponseType(typeof(Guid), 202)]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest req, CancellationToken ct = default)
    {
        var ok = await update.HandleAsync(
            new(req.Id, req.Name, req.NameRu, req.NameEn, req.NameKa, req.Code,
                req.InternationalCode, req.InternationalName, req.AlternativeName, req.Type), ct);
        return ok ? Ok(req.Id) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct = default)
    {
        var (notFound, error) = await delete.HandleAsync(new(id), ct);
        if (notFound) return NotFound();
        return error is not null ? Conflict(error) : NoContent();
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteProducts([FromBody] Guid[] ids, CancellationToken ct = default)
    {
        var (_, error) = await deleteRange.HandleAsync(new(ids), ct);
        return error is not null ? Conflict(error) : NoContent();
    }
}
