namespace Ucms.Application.Features.ClientActs.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteClientAct
{
    public record Command(Guid ProjectId, Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool ProjectNotFound, bool Forbidden, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (false, true, false, null);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (false, false, true, null);

            var act = await db.ClientActs
                .FirstOrDefaultAsync(a => a.Id == cmd.Id && a.ProjectId == cmd.ProjectId, ct);

            if (act is null) return (true, false, false, null);

            if (act.Status != ActStatus.Draft)
                return (false, false, false, "Faqat Draft holatidagi aktni o'chirish mumkin");

            db.ClientActs.Remove(act);
            await db.SaveChangesAsync(ct);
            return (false, false, false, null);
        }
    }
}
