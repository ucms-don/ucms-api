namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Organizations.Commands;
using Ucms.Application.Features.Organizations.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Tashkilotlarni boshqarish.
/// Управление организациями.
/// </summary>
[ApiController]
[Route("api/organizations")]
[Tags("Organization")]
[Authorize]
public class OrganizationController(
    GetOrganizations.Handler    getAll,
    GetOrganizationById.Handler getById,
    CreateOrganization.Handler  create,
    UpdateOrganization.Handler  update,
    DeleteOrganization.Handler  delete) : ControllerBase
{
    public record CreateOrganizationRequest(
        string Name, string? TaxId, string? Address, string? Phone, string? Email,
        OrganizationType Type = OrganizationType.Tenant,
        bool IsTest = false);

    public record UpdateOrganizationRequest(
        string Name, string? TaxId, string? Address, string? Phone, string? Email,
        bool? IsTest = null);

    /// <summary>
    /// Barcha tashkilotlar ro'yxati.
    /// Список всех организаций.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await getAll.HandleAsync(new(), ct));
    }

    /// <summary>
    /// ID bo'yicha tashkilotni olish.
    /// Получить организацию по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var (data, forbidden) = await getById.HandleAsync(new(id), ct);
        if (forbidden) return Forbid();
        return data is null ? NotFound() : Ok(data);
    }

    /// <summary>
    /// Yangi tashkilot yaratish. Faqat Admin uchun.
    /// Создать новую организацию. Только для Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(201)]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest req, CancellationToken ct)
    {
        var result = await create.HandleAsync(
            new(req.Name, req.TaxId, req.Address, req.Phone, req.Email, req.Type, req.IsTest), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Tashkilot ma'lumotlarini yangilash. Admin yoki Manager uchun.
    /// Обновить данные организации. Для Admin или Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizationRequest req, CancellationToken ct)
    {
        var (notFound, forbidden) = await update.HandleAsync(
            new(id, req.Name, req.TaxId, req.Address, req.Phone, req.Email, req.IsTest), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return NoContent();
    }

    /// <summary>
    /// Tashkilotni o'chirish. Faqat Admin uchun.
    /// Удалить организацию. Только для Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await delete.HandleAsync(new(id), ct) ? NoContent() : NotFound();
    }
}
