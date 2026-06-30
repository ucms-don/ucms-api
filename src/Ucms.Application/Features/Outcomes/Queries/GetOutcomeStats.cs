namespace Ucms.Application.Features.Outcomes.Queries;

using AutoMapper;
using Ucms.Application.Abstractions.Organization;
using Ucms.Application.Persistence;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Features.Outcomes.DTOs;

public static class GetOutcomeStats
{
    public record Query(Guid OrganizationId, DateTime From, DateTime To, DateTime PreviousFrom, DateTime PreviousTo);

    public sealed class Handler(IUcmsDbContext db, IOrganizationClient organizationClient, IMapper mapper)
    {
        public async Task<OutcomeStatsModel> HandleAsync(Query q, CancellationToken ct)
        {
            var orgIds = await organizationClient.GetOrganizationIds(q.OrganizationId);
            var items = db.OutcomeItems.Where(w => orgIds.Contains(w.Outcome!.Stock!.OrganizationId));

            var current  = items.Where(w => w.Outcome!.OutcomeDate > q.From && w.Outcome!.OutcomeDate < q.To);
            var previous = items.Where(w => w.Outcome!.OutcomeDate > q.PreviousFrom && w.Outcome!.OutcomeDate < q.PreviousTo);

            var curData  = current.GroupBy(g => g.Sku).Select(s => new { Sku = s.Key, Count = s.Count() }).OrderBy(o => o.Count).Take(10).ToList();
            var prevData = previous.GroupBy(g => g.Sku).Select(s => new { Sku = s.Key, Count = s.Count() }).OrderBy(o => o.Count).Take(10).ToList();

            return new OutcomeStatsModel(
                curData.Select(p => new OutcomeStatItemModel(mapper.Map<SkuModel>(p.Sku), p.Count)).ToList(),
                prevData.Select(p => new OutcomeStatItemModel(mapper.Map<SkuModel>(p.Sku), p.Count)).ToList());
        }
    }
}
