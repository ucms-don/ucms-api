namespace Ucms.Application.Features.ClientActs.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateClientAct
{
    public record ActItemDto(Guid EstimateItemId, decimal Volume, decimal UnitPrice);

    public record Command(
        Guid ProjectId,
        string ActNumber,
        DateTimeOffset ActDate,
        List<ActItemDto> Items,
        string? Note);

    public record Result(Guid Id, string ActNumber, decimal TotalAmount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == cmd.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var actId       = Guid.NewGuid();
            var totalAmount = cmd.Items.Sum(i => i.Volume * i.UnitPrice);
            var now         = DateTimeOffset.UtcNow;
            var userId      = ctx.UserId ?? Guid.Empty;

            var act = new ClientAct
            {
                Id          = actId,
                ProjectId   = cmd.ProjectId,
                ActNumber   = cmd.ActNumber,
                ActDate     = cmd.ActDate,
                TotalAmount = totalAmount,
                Status      = ActStatus.Draft,
                Note        = cmd.Note,
                CreatedAt   = now, UpdatedAt = now,
                CreatedBy   = userId, UpdatedBy = userId,
            };

            var items = cmd.Items.Select(i => new ClientActItem
            {
                Id             = Guid.NewGuid(),
                ActId          = actId,
                EstimateItemId = i.EstimateItemId,
                Volume         = i.Volume,
                UnitPrice      = i.UnitPrice,
                TotalAmount    = i.Volume * i.UnitPrice,
            }).ToList();

            await db.ClientActs.AddAsync(act, ct);
            await db.ClientActItems.AddRangeAsync(items, ct);
            await db.SaveChangesAsync(ct);
            return (new Result(actId, act.ActNumber, act.TotalAmount), false, false);
        }
    }
}
