namespace Ucms.Application.Features.WorkLogs.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class RejectWorkLogs
{
    public record Command(Guid ProjectId, Guid[] WorkLogIds, string? Reason);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(int Rejected, bool ProjectNotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (0, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (0, false, true);

            var workLogs = await db.WorkLogs
                .Where(w => cmd.WorkLogIds.Contains(w.Id)
                         && w.ProjectId == cmd.ProjectId
                         && (w.Status == WorkLogStatus.Draft || w.Status == WorkLogStatus.Confirmed))
                .ToListAsync(ct);

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            foreach (var wl in workLogs)
            {
                wl.Status    = WorkLogStatus.Rejected;
                wl.Note      = cmd.Reason ?? wl.Note;
                wl.UpdatedAt = now;
                wl.UpdatedBy = userId;
                db.WorkLogs.Update(wl);
            }

            await db.SaveChangesAsync(ct);
            return (workLogs.Count, false, false);
        }
    }
}
