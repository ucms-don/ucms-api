namespace Ucms.Application.Features.WorkTypes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteWorkType
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var entity = await db.WorkTypes.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
            if (entity is null) return false;
            entity.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
