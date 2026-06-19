namespace Ucms.Application.Features.WorkLogs.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateWorkLog
{
    public record Command(
        Guid ProjectId, Guid Id,
        DateTimeOffset Date, decimal Volume, decimal BrigadeUnitPrice,
        string? Floor, string? Zone, string? Room, string? Note);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var workLog = await db.WorkLogs
                .FirstOrDefaultAsync(w => w.Id == cmd.Id && w.ProjectId == cmd.ProjectId, ct);

            if (workLog is null) return (true, false, null);

            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (false, true, null);

            if (workLog.Status != WorkLogStatus.Draft)
                return (false, false, "Faqat Draft holatidagi yozuvni o'zgartirish mumkin");

            var smetaVolume = await db.EstimateItems
                .Where(i => i.Id == workLog.EstimateItemId)
                .Select(i => (decimal?)i.Volume)
                .FirstOrDefaultAsync(ct) ?? 0m;

            var usedVolume = await db.WorkLogs
                .Where(w => w.EstimateItemId == workLog.EstimateItemId
                         && w.ProjectId      == cmd.ProjectId
                         && w.Id             != cmd.Id
                         && w.Status         != WorkLogStatus.Rejected)
                .SumAsync(w => (decimal?)w.Volume, ct) ?? 0m;

            if (usedVolume + cmd.Volume > smetaVolume)
                return (false, false,
                    $"Kiritilgan hajm ({cmd.Volume}) smetadagi ruxsat etilgan hajmdan oshib ketadi. " +
                    $"Smeta bo'yicha: {smetaVolume}, boshqa yozuvlarda: {usedVolume}, " +
                    $"qolgan: {smetaVolume - usedVolume}");

            workLog.Date             = cmd.Date;
            workLog.Volume           = cmd.Volume;
            workLog.BrigadeUnitPrice = cmd.BrigadeUnitPrice;
            workLog.TotalAmount      = cmd.Volume * cmd.BrigadeUnitPrice;
            workLog.Floor            = cmd.Floor;
            workLog.Zone             = cmd.Zone;
            workLog.Room             = cmd.Room;
            workLog.Note             = cmd.Note;
            workLog.UpdatedAt        = DateTimeOffset.UtcNow;
            workLog.UpdatedBy        = ctx.UserId ?? Guid.Empty;

            db.WorkLogs.Update(workLog);
            await db.SaveChangesAsync(ct);
            return (false, false, null);
        }
    }
}
