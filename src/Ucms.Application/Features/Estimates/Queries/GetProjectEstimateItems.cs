namespace Ucms.Application.Features.Estimates.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

/// <summary>
/// Loyiha bo'yicha barcha smeta pozitsiyalarini tekis (flat) ro'yxat sifatida qaytaradi.
/// WorkLog yaratishda estimateItemId tanlash uchun ishlatiladi.
/// </summary>
public static class GetProjectEstimateItems
{
    public record Query(Guid ProjectId);

    public record ItemOption(
        Guid    Id,
        Guid    WorkTypeId,
        string  WorkTypeName,
        string  EstimateName,
        string  SectionName,
        string  MeasurementUnitCode,
        decimal BrigadeUnitPrice);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(List<ItemOption>? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, true);

            var locale = ctx.Locale;
            var items = await db.EstimateItems
                .Where(i => i.Section!.Estimate!.ProjectId == q.ProjectId)
                .OrderBy(i => i.Section!.Estimate!.Order)
                .ThenBy(i => i.Section!.Order)
                .ThenBy(i => i.Order)
                .Select(i => new ItemOption(
                    i.Id,
                    i.WorkTypeId!.Value,
                    locale == "ru" ? i.WorkType!.NameRu
                  : locale == "en" ? (i.WorkType!.NameEn ?? i.WorkType!.Name)
                  : locale == "ka" ? (i.WorkType!.NameKa ?? i.WorkType!.Name)
                  : i.WorkType!.Name,
                    i.Section!.Estimate!.Name,
                    i.Section!.Name,
                    i.MeasurementUnit!.Code,
                    i.BrigadeUnitPrice))
                .ToListAsync(ct);

            return (items, false);
        }
    }
}
