namespace Ucms.Application.Features.Estimates.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteSection
{
    public record Command(Guid ProjectId, Guid EstimateId, Guid SectionId);

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

            // Bo'lim va uning barcha ichki (avlod) bo'limlarini yig'amiz — self-FK buzilmasligi uchun
            var estimateSections = await db.EstimateSections
                .Where(s => s.EstimateId == cmd.EstimateId)
                .Select(s => new { s.Id, s.ParentId })
                .ToListAsync(ct);

            var toDelete = new HashSet<Guid> { section.Id };
            bool added;
            do
            {
                added = false;
                foreach (var s in estimateSections)
                {
                    if (s.ParentId is Guid pid && toDelete.Contains(pid) && toDelete.Add(s.Id))
                        added = true;
                }
            } while (added);

            // Avlodlardan ildizgacha o'chiramiz (chuqurroq bo'limlar avval) — FK cheklovi buzilmaydi
            var sections = await db.EstimateSections
                .Where(s => toDelete.Contains(s.Id))
                .ToListAsync(ct);

            db.EstimateSections.RemoveRange(sections);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
