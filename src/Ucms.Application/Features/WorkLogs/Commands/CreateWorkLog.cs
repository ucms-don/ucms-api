namespace Ucms.Application.Features.WorkLogs.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateWorkLog
{
    public record Command(
        Guid ProjectId,
        Guid BrigadeId,
        Guid EstimateItemId,
        DateTimeOffset Date,
        decimal Volume,
        decimal? BrigadeUnitPrice,
        string? Floor,
        string? Zone,
        string? Room,
        string? Note);

    public record Result(Guid Id, decimal TotalAmount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool ProjectNotFound, bool Forbidden, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false, null);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true, null);

            var estimateItem = await db.EstimateItems
                .Where(i => i.Id == cmd.EstimateItemId)
                .Select(i => new { i.BrigadeUnitPrice, i.Volume })
                .FirstOrDefaultAsync(ct);

            if (estimateItem is null)
                return (null, false, false, "Smeta qatori topilmadi");

            var usedVolume = await db.WorkLogs
                .Where(w => w.EstimateItemId == cmd.EstimateItemId
                         && w.ProjectId      == cmd.ProjectId
                         && w.Status         != WorkLogStatus.Rejected)
                .SumAsync(w => (decimal?)w.Volume, ct) ?? 0m;

            if (usedVolume + cmd.Volume > estimateItem.Volume)
                return (null, false, false,
                    $"Kiritilgan hajm ({cmd.Volume}) smetadagi ruxsat etilgan hajmdan oshib ketadi. " +
                    $"Smeta bo'yicha: {estimateItem.Volume}, allaqachon kiritilgan: {usedVolume}, " +
                    $"qolgan: {estimateItem.Volume - usedVolume}");

            var unitPrice   = cmd.BrigadeUnitPrice ?? estimateItem.BrigadeUnitPrice;
            var totalAmount = cmd.Volume * unitPrice;
            var now         = DateTimeOffset.UtcNow;
            var userId      = ctx.UserId ?? Guid.Empty;

            var workLog = new WorkLog
            {
                Id               = Guid.NewGuid(),
                ProjectId        = cmd.ProjectId,
                BrigadeId        = cmd.BrigadeId,
                EstimateItemId   = cmd.EstimateItemId,
                Date             = cmd.Date,
                Volume           = cmd.Volume,
                BrigadeUnitPrice = unitPrice,
                TotalAmount      = totalAmount,
                Floor            = cmd.Floor,
                Zone             = cmd.Zone,
                Room             = cmd.Room,
                Note             = cmd.Note,
                Status           = WorkLogStatus.Draft,
                CreatedAt        = now, UpdatedAt = now,
                CreatedBy        = userId, UpdatedBy = userId,
            };

            await db.WorkLogs.AddAsync(workLog, ct);
            await db.SaveChangesAsync(ct);
            return (new Result(workLog.Id, workLog.TotalAmount), false, false, null);
        }
    }
}
