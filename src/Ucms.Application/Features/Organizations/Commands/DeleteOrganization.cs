namespace Ucms.Application.Features.Organizations.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteOrganization
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var org = await db.Organizations.FindAsync([cmd.Id], ct);
            if (org is null || org.IsDeleted) return false;

            org.IsDeleted = true;
            org.UpdatedAt = DateTimeOffset.UtcNow;
            org.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Organizations.Update(org);
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
