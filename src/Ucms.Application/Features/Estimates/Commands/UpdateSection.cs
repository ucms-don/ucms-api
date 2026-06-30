namespace Ucms.Application.Features.Estimates.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class UpdateSection
{
    public record Command(Guid ProjectId, Guid EstimateId, Guid SectionId, string Name, int Order);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null || (!ctx.IsOwner && ctx.OrganizationId != orgId))
                return (false, orgId is not null);

            var section = await db.EstimateSections
                .FirstOrDefaultAsync(s => s.Id == cmd.SectionId && s.EstimateId == cmd.EstimateId, ct);

            if (section is null) return (true, false);

            section.Name  = cmd.Name;
            section.Order = cmd.Order;

            db.EstimateSections.Update(section);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
