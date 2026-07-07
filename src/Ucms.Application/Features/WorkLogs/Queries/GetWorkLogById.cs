namespace Ucms.Application.Features.WorkLogs.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.WorkLogs.DTOs;
using Ucms.Application.Persistence;

public static class GetWorkLogById
{
    public record Query(Guid ProjectId, Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(WorkLogDetailDto? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var locale = ctx.Locale;
            var workLog = await db.WorkLogs
                .Where(w => w.Id == q.Id && w.ProjectId == q.ProjectId)
                .Select(w => new WorkLogDetailDto(
                    w.Id,
                    w.Date,
                    w.Volume,
                    w.BrigadeUnitPrice,
                    w.TotalAmount,
                    w.Status,
                    w.Floor,
                    w.Zone,
                    w.Room,
                    w.Note,
                    w.BrigadePaymentId,
                    w.CreatedAt,
                    w.UpdatedAt,
                    new WorkLogBrigadeDto(w.Brigade!.Id, w.Brigade.Name),
                    new WorkLogDetailEstimateItemDto(
                        w.EstimateItem!.Id,
                        locale == "ru" ? w.EstimateItem.WorkType!.NameRu
                      : locale == "en" ? (w.EstimateItem.WorkType!.NameEn ?? w.EstimateItem.WorkType!.Name)
                      : locale == "ka" ? (w.EstimateItem.WorkType!.NameKa ?? w.EstimateItem.WorkType!.Name)
                      : w.EstimateItem.WorkType!.Name,
                        w.EstimateItem.MeasurementUnit!.Code)))
                .FirstOrDefaultAsync(ct);

            return (workLog, false, false);
        }
    }
}
