namespace Ucms.Application.Features.Estimates.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateEstimate
{
    public record Command(Guid ProjectId, string Name, string? Description, int Order);
    public record Result(Guid Id, string Name, string? Description, int Order);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null || (!ctx.IsOwner && ctx.OrganizationId != orgId))
                return (null, orgId is not null);

            var estimate = new Estimate
            {
                Id          = Guid.NewGuid(),
                ProjectId   = cmd.ProjectId,
                Name        = cmd.Name,
                Description = cmd.Description,
                Order       = cmd.Order,
            };

            await db.Estimates.AddAsync(estimate, ct);
            await db.SaveChangesAsync(ct);
            return (new Result(estimate.Id, estimate.Name, estimate.Description, estimate.Order), false);
        }
    }
}
