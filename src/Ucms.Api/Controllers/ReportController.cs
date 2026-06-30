namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.Reports.DTOs;
using Ucms.Application.Features.Reports.Queries;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportController(
    GetProductBalanceReport.Handler getReport,
    GetProductBalanceExcel.Handler getExcel) : ControllerBase
{
    [HttpGet("product-balance")]
    [ProducesResponseType(typeof(ProductBalanceReportModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductBalance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid organizationId,
        CancellationToken ct)
        {
            return Ok(await getReport.HandleAsync(new(from, to, organizationId), ct));
        }

    [HttpPost("export-product-balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportProductBalance([FromBody] ProductBalanceReportModel data, CancellationToken ct)
    {
        var stream = await getExcel.HandleAsync(new(data), ct);
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Отчет остатков по периодам.xlsx");
    }
}
