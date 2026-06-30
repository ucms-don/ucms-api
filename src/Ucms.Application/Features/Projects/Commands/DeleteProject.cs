namespace Ucms.Application.Features.Projects.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteProject
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var project = await db.Projects.FindAsync([cmd.Id], ct);
            if (project is null || project.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != project.OrganizationId) return (false, true);

            project.IsDeleted = true;
            project.UpdatedAt = DateTimeOffset.UtcNow;
            project.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Projects.Update(project);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
