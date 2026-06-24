namespace Ucms.Application.Features.WorkLogs.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteWorkLog
{
    public record Command(Guid ProjectId, Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var workLog = await db.WorkLogs
                .FirstOrDefaultAsync(w => w.Id == cmd.Id && w.ProjectId == cmd.ProjectId, ct);

            if (workLog is null) return (true, null);

            if (workLog.Status != WorkLogStatus.Draft)
                return (false, "Faqat Draft holatidagi yozuvni o'chirish mumkin");

            db.WorkLogs.Remove(workLog);
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
