namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.WorkLogs.Queries;
using Ucms.Domain.Enums;

/// <summary>
/// Barcha loyihalar bo'yicha ish jurnali (global ro'yxat).
/// Глобальный журнал работ по всем проектам.
/// </summary>
[ApiController]
[Route("api/worklogs")]
[Tags("WorkLog")]
[Authorize]
public class GlobalWorkLogController(
    GetAllWorkLogs.Handler getAll) : ControllerBase
{
    /// <summary>
    /// Ish jurnallari global ro'yxati (loyiha, brigada, status, sana filtrlari bilan).
    /// Глобальный список журналов работ (с фильтрами по проекту, бригаде, статусу и дате).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? brigadeId,
        [FromQuery] WorkLogStatus? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(
            new(projectId, brigadeId, status, from, to, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }
}
