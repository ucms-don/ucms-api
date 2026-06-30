namespace Ucms.Application.Features.Stats.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Dashboards;
using Ucms.Application.Abstractions.Organization;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetAmbulanceStocksStats
{
    public record Query(Guid? OrganizationId, Guid? RegionId, Guid? CityId);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, IOrganizationClient organizationClient)
    {
        public async Task<DashboardWidgetModel> HandleAsync(Query q, CancellationToken ct)
        {
            var organizationId = q.OrganizationId ?? workContext.TenantId;
            var orgIds = await organizationClient.GetOrganizationIds(organizationId, true, q.RegionId, q.CityId);
            var allIds = orgIds.Append(organizationId);

            var stockSkus = db.StockSkus;

            var byCat = await stockSkus
                .Where(w => allIds.Contains(w.Stock!.OrganizationId) && w.Stock.StockType != StockType.Case)
                .GroupBy(g => g.Stock!.StockCategory)
                .Select(s => new { Category = s.Key, Amount = s.Sum(x => x.Amount) })
                .ToDictionaryAsync(d => d.Category, d => d.Amount, ct);

            var caseAmount = await stockSkus
                .Where(w => allIds.Contains(w.Stock!.OrganizationId) && w.Stock.StockType == StockType.Case)
                .SumAsync(x => x.Amount, ct);

            var widget = new DashboardWidgetModel
            {
                Title = "MAVJUD DORI VOSITALARI", TitleRu = "ДОСТУПНЫЕ ЛЕКАРСТВА",
                TitleEn = "AVAILABLE MEDICINES", TitleKa = "MAVJUD DORI VOSITALARI",
                Items =
                [
                    new() { Title = "Asosiy omborlarda", TitleRu = "На основных складах",
                            TitleEn = "In the main warehouses", TitleKa = "Asosiy omborlarda",
                            Count = (int)byCat.GetValueOrDefault(StockCategory.Central) },
                    new() { Title = "Podstansiyalarda", TitleRu = "На подстанциях",
                            TitleEn = "In substations", TitleKa = "Podstansiyalarda",
                            Count = (int)byCat.GetValueOrDefault(StockCategory.Default) },
                    new() { Title = "Sumkalarda", TitleRu = "В сумках",
                            TitleEn = "In cases", TitleKa = "Sumkalarda",
                            Count = (int)caseAmount }
                ]
            };
            widget.TotalCount = widget.Items.Sum(x => x.Count);
            return widget;
        }
    }
}
