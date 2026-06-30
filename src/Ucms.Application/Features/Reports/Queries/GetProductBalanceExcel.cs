namespace Ucms.Application.Features.Reports.Queries;

using Ucms.Application.Features.Reports.DTOs;
using Ucms.Application.Services;

public static class GetProductBalanceExcel
{
    public record Query(ProductBalanceReportModel Data);

    public sealed class Handler(IProductBalanceReportService service)
    {
        public async Task<MemoryStream> HandleAsync(Query q, CancellationToken ct)
            => await service.GetExcelAsync(q.Data, ct);
    }
}
