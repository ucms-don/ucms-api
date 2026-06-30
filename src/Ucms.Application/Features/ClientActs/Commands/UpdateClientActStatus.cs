namespace Ucms.Application.Features.ClientActs.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateClientActStatus
{
    public record Command(Guid ProjectId, Guid Id, ActStatus Status);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool ProjectNotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (false, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (false, false, true);

            var act = await db.ClientActs
                .FirstOrDefaultAsync(a => a.Id == cmd.Id && a.ProjectId == cmd.ProjectId, ct);

            if (act is null) return (true, false, false);

            act.Status    = cmd.Status;
            act.UpdatedAt = DateTimeOffset.UtcNow;
            act.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.ClientActs.Update(act);
            await db.SaveChangesAsync(ct);
            return (false, false, false);
        }
    }
}
