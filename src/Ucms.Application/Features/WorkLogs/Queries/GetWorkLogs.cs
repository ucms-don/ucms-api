namespace Ucms.Application.Features.WorkLogs.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.WorkLogs.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetWorkLogs
{
    public record Query(
        Guid ProjectId,
        Guid? BrigadeId,
        WorkLogStatus? Status,
        DateTimeOffset? From,
        DateTimeOffset? To,
        int Page,
        int Size);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(WorkLogPagedResult? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var query = db.WorkLogs.Where(w => w.ProjectId == q.ProjectId);

            if (q.BrigadeId.HasValue) query = query.Where(w => w.BrigadeId == q.BrigadeId.Value);
            if (q.Status.HasValue)    query = query.Where(w => w.Status == q.Status.Value);
            if (q.From.HasValue)      query = query.Where(w => w.Date >= q.From.Value);
            if (q.To.HasValue)        query = query.Where(w => w.Date <= q.To.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(w => w.Date)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(w => new WorkLogDto(
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
                    w.Brigade!.Name,
                    new WorkLogEstimateItemDto(
                        w.EstimateItem!.WorkType!.Name,
                        w.EstimateItem.MeasurementUnit!.Code)))
                .ToListAsync(ct);

            return (new WorkLogPagedResult(total, q.Page, q.Size, items), false, false);
        }
    }
}
