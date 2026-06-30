namespace Ucms.Application.Features.Brigades.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteBrigade
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var brigade = await db.Brigades.FindAsync([cmd.Id], ct);
            if (brigade is null || brigade.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != brigade.OrganizationId) return (false, true);

            brigade.IsDeleted = true;
            brigade.IsActive  = false;
            brigade.UpdatedAt = DateTimeOffset.UtcNow;
            brigade.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Brigades.Update(brigade);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
