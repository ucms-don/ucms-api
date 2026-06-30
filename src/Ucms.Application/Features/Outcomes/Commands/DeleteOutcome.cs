namespace Ucms.Application.Features.Outcomes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteOutcome
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var outcome = await db.Outcomes.Include(i => i.OutcomeItems)
                .AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (outcome is null) return false;
            foreach (var item in outcome.OutcomeItems) item.IsDeleted = true;
            outcome.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
