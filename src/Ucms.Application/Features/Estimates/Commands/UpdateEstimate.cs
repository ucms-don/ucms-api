namespace Ucms.Application.Features.Estimates.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class UpdateEstimate
{
    public record Command(Guid ProjectId, Guid EstimateId, string Name, string? Description, int Order);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null || (!ctx.IsOwner && ctx.OrganizationId != orgId))
                return (false, orgId is not null);

            var estimate = await db.Estimates
                .FirstOrDefaultAsync(e => e.Id == cmd.EstimateId && e.ProjectId == cmd.ProjectId, ct);

            if (estimate is null) return (true, false);

            estimate.Name        = cmd.Name;
            estimate.Description = cmd.Description;
            estimate.Order       = cmd.Order;

            db.Estimates.Update(estimate);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
