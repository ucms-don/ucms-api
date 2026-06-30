namespace Ucms.Application.Services;

using Ucms.Application.Features.Reports.DTOs;

public interface IProductBalanceReportService
{
    public Task<MemoryStream> GetExcelAsync(ProductBalanceReportModel data, CancellationToken cancellationToken = default);
}
