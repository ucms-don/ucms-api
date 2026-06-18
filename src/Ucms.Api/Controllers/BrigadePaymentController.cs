namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Payments.Queries;

/// <summary>
/// Barcha loyihalar bo'yicha brigada to'lovlari (global ro'yxat).
/// Глобальный список выплат бригадам по всем проектам.
/// </summary>
[ApiController]
[Route("api/brigade-payments")]
[Tags("BrigadePayment")]
[Authorize]
public class BrigadePaymentController(
    GetAllBrigadePayments.Handler getAll) : ControllerBase
{
    /// <summary>
    /// Barcha brigada to'lovlari ro'yxati (brigada, loyiha, sana filtrlari bilan).
    /// Список всех выплат бригадам (с фильтрами по бригаде, проекту и дате).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? brigadeId,
        [FromQuery] Guid? projectId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        var (data, forbidden) = await getAll.HandleAsync(new(brigadeId, projectId, from, to, page, size), ct);
        if (forbidden) return Forbid();
        return Ok(data);
    }
}
