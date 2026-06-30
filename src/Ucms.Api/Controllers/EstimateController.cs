namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Estimates.Commands;
using Ucms.Application.Features.Estimates.Queries;
using Ucms.Domain.Enums;
using GetProjectEstimateItemsQuery = Ucms.Application.Features.Estimates.Queries.GetProjectEstimateItems;

/// <summary>
/// Loyiha smeta hujjatlarini boshqarish.
/// Иерархия: Project → Estimates → Sections → Items
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/estimates")]
[Tags("Estimate")]
[Authorize(Roles = "Admin,Manager")]
public class EstimateController(
    GetEstimates.Handler   getEstimates,
    CreateEstimate.Handler createEstimate,
    UpdateEstimate.Handler updateEstimate,
    DeleteEstimate.Handler deleteEstimate,
    GetSections.Handler    getSections,
    CreateSection.Handler  createSection,
    UpdateSection.Handler  updateSection,
    DeleteSection.Handler  deleteSection,
    GetItems.Handler       getItems,
    CreateItem.Handler     createItem,
    UpdateItem.Handler     updateItem,
    DeleteItem.Handler     deleteItem,
    GetProjectEstimateItemsQuery.Handler getProjectItems) : ControllerBase
{
    public record CreateEstimateRequest(string Name, string? Description, int Order);
    public record UpdateEstimateRequest(string Name, string? Description, int Order);
    public record CreateSectionRequest(string Name, int Order, Guid? ParentId);
    public record UpdateSectionRequest(string Name, int Order);
    public record CreateItemRequest(
        Guid SectionId, Guid WorkTypeId, SurfaceType? SurfaceType, string? Description, Guid MeasurementUnitId, decimal Volume,
        decimal ClientUnitPrice, decimal BrigadeUnitPrice, decimal MaterialUnitPrice, decimal VatRate, int Order);
    public record UpdateItemRequest(
        Guid WorkTypeId, SurfaceType? SurfaceType, string? Description, Guid MeasurementUnitId, decimal Volume,
        decimal ClientUnitPrice, decimal BrigadeUnitPrice, decimal MaterialUnitPrice, decimal VatRate, int Order);

    // ── Flat items (WorkLog uchun) ─────────────────────────────────────────────

    /// <summary>
    /// Loyihaning barcha smeta pozitsiyalari (tekis ro'yxat). WorkLog yaratishda ishlatiladi.
    /// </summary>
    [HttpGet("items")]
    [Authorize(Roles = "Admin,Manager,Brigadir,Accountant")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectEstimateItems(Guid projectId, CancellationToken ct)
    {
        var (data, forbidden) = await getProjectItems.HandleAsync(new(projectId), ct);
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    // ── Estimates ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Loyiha smeta hujjatlari ro'yxati.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Brigadir,Accountant")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEstimates(Guid projectId, CancellationToken ct)
    {
        var (data, forbidden) = await getEstimates.HandleAsync(new(projectId), ct);
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Yangi smeta hujjati yaratish.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateEstimate(
        Guid projectId, [FromBody] CreateEstimateRequest req, CancellationToken ct)
    {
        var (data, forbidden) = await createEstimate.HandleAsync(
            new(projectId, req.Name, req.Description, req.Order), ct);
        if (forbidden) return Forbid();
        if (data is null) return NotFound();
        return StatusCode(201, data);
    }

    /// <summary>
    /// Smeta hujjatini yangilash.
    /// </summary>
    [HttpPut("{estimateId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateEstimate(
        Guid projectId, Guid estimateId, [FromBody] UpdateEstimateRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await updateEstimate.HandleAsync(
            new(projectId, estimateId, req.Name, req.Description, req.Order), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Smeta hujjatini o'chirish (soft delete).
    /// </summary>
    [HttpDelete("{estimateId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteEstimate(
        Guid projectId, Guid estimateId, CancellationToken ct)
    {
        var (notFound, forbidden) = await deleteEstimate.HandleAsync(new(projectId, estimateId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    // ── Sections ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Smeta bo'limlari ro'yxati.
    /// </summary>
    [HttpGet("{estimateId:guid}/sections")]
    [Authorize(Roles = "Admin,Manager,Brigadir,Accountant")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSections(
        Guid projectId, Guid estimateId, CancellationToken ct)
    {
        var (data, forbidden) = await getSections.HandleAsync(new(projectId, estimateId), ct);
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Yangi smeta bo'limi yaratish.
    /// </summary>
    [HttpPost("{estimateId:guid}/sections")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateSection(
        Guid projectId, Guid estimateId, [FromBody] CreateSectionRequest req, CancellationToken ct)
    {
        var (data, forbidden) = await createSection.HandleAsync(
            new(projectId, estimateId, req.Name, req.Order, req.ParentId), ct);
        if (forbidden) return Forbid();
        if (data is null) return NotFound();
        return StatusCode(201, data);
    }

    /// <summary>
    /// Smeta bo'limini yangilash.
    /// </summary>
    [HttpPut("{estimateId:guid}/sections/{sectionId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSection(
        Guid projectId, Guid estimateId, Guid sectionId,
        [FromBody] UpdateSectionRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await updateSection.HandleAsync(
            new(projectId, estimateId, sectionId, req.Name, req.Order), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Smeta bo'limini o'chirish.
    /// </summary>
    [HttpDelete("{estimateId:guid}/sections/{sectionId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteSection(
        Guid projectId, Guid estimateId, Guid sectionId, CancellationToken ct)
    {
        var (notFound, forbidden) = await deleteSection.HandleAsync(
            new(projectId, estimateId, sectionId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    // ── Items ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Bo'lim ichidagi pozitsiyalar ro'yxati.
    /// </summary>
    [HttpGet("{estimateId:guid}/sections/{sectionId:guid}/items")]
    [Authorize(Roles = "Admin,Manager,Brigadir,Accountant")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItems(
        Guid projectId, Guid estimateId, Guid sectionId, CancellationToken ct)
    {
        var (data, forbidden) = await getItems.HandleAsync(new(projectId, estimateId, sectionId), ct);
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Yangi smeta pozitsiyasi yaratish.
    /// </summary>
    [HttpPost("{estimateId:guid}/items")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateItem(
        Guid projectId, Guid estimateId, [FromBody] CreateItemRequest req, CancellationToken ct)
    {
        var (data, forbidden, error) = await createItem.HandleAsync(
            new(projectId, estimateId, req.SectionId, req.WorkTypeId, req.SurfaceType, req.Description, req.MeasurementUnitId, req.Volume,
                req.ClientUnitPrice, req.BrigadeUnitPrice, req.MaterialUnitPrice, req.VatRate, req.Order), ct);
        if (forbidden)         return Forbid();
        if (error is not null) return BadRequest(new { message = error });
        if (data is null)      return NotFound();
        return StatusCode(201, data);
    }

    /// <summary>
    /// Smeta pozitsiyasini yangilash.
    /// </summary>
    [HttpPut("{estimateId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateItem(
        Guid projectId, Guid estimateId, Guid itemId,
        [FromBody] UpdateItemRequest req, CancellationToken ct)
    {
        var (notFound, forbidden, error) = await updateItem.HandleAsync(
            new(projectId, estimateId, itemId, req.WorkTypeId, req.SurfaceType, req.Description, req.MeasurementUnitId, req.Volume,
                req.ClientUnitPrice, req.BrigadeUnitPrice, req.MaterialUnitPrice, req.VatRate, req.Order), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        if (error is not null) return BadRequest(new { message = error });
        return NoContent();
    }

    /// <summary>
    /// Smeta pozitsiyasini o'chirish.
    /// </summary>
    [HttpDelete("{estimateId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteItem(
        Guid projectId, Guid estimateId, Guid itemId, CancellationToken ct)
    {
        var (notFound, forbidden) = await deleteItem.HandleAsync(new(projectId, estimateId, itemId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }
}
