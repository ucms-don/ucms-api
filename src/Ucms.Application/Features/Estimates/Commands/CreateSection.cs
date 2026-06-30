namespace Ucms.Application.Features.Estimates.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateSection
{
    public record Command(Guid ProjectId, Guid EstimateId, string Name, int Order, Guid? ParentId);
    public record Result(Guid Id, string Name, int Order, Guid? ParentId);

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

            var estimateExists = await db.Estimates
                .AnyAsync(e => e.Id == cmd.EstimateId && e.ProjectId == cmd.ProjectId, ct);

            if (!estimateExists) return (null, false);

            if (cmd.ParentId is not null)
            {
                var parentValid = await db.EstimateSections
                    .AnyAsync(s => s.Id == cmd.ParentId && s.EstimateId == cmd.EstimateId, ct);
                if (!parentValid) return (null, false);
            }

            var section = new EstimateSection
            {
                Id         = Guid.NewGuid(),
                EstimateId = cmd.EstimateId,
                Name       = cmd.Name,
                Order      = cmd.Order,
                ParentId   = cmd.ParentId,
            };

            await db.EstimateSections.AddAsync(section, ct);
            await db.SaveChangesAsync(ct);
            return (new Result(section.Id, section.Name, section.Order, section.ParentId), false);
        }
    }
}
