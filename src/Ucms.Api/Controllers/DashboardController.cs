namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Dashboard.DTOs;
using Ucms.Application.Features.Dashboard.Queries;

/// <summary>
/// Bosh sahifa statistikasi va loyiha tafsilotlari.
/// Статистика главной страницы и детали проекта.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Tags("Dashboard")]
[Authorize]
public class DashboardController(
    GetDashboard.Handler     getDashboard,
    GetProjectDetail.Handler getProjectDetail) : ControllerBase
{
    /// <summary>
    /// Umumiy dashboard ma'lumotlari (tashkilot kontekstida).
    /// Общие данные дашборда (в контексте организации).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), 200)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        return Ok(await getDashboard.HandleAsync(new(), ct));
    }

    /// <summary>
    /// Bitta loyiha bo'yicha batafsil dashboard ma'lumotlari.
    /// Детальные данные дашборда по одному проекту.
    /// </summary>
    [HttpGet("projects/{projectId:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectDetail(Guid projectId, CancellationToken ct)
    {
        var (data, notFound, forbidden) = await getProjectDetail.HandleAsync(new(projectId), ct);
        if (notFound)  return NotFound();
        if (forbidden) return Forbid();
        return Ok(data);
    }
}
