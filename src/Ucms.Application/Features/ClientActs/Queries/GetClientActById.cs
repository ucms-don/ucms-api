namespace Ucms.Application.Features.ClientActs.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.ClientActs.DTOs;
using Ucms.Application.Persistence;

public static class GetClientActById
{
    public record Query(Guid ProjectId, Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(ClientActDetailDto? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var act = await db.ClientActs
                .Where(a => a.Id == q.Id && a.ProjectId == q.ProjectId)
                .Select(a => new ClientActDetailDto(
                    a.Id,
                    a.ActNumber,
                    a.ActDate,
                    a.TotalAmount,
                    a.Status,
                    a.Note,
                    a.CreatedAt,
                    a.UpdatedAt,
                    a.Items.Select(i => new ClientActItemDto(
                        i.Id,
                        i.EstimateItemId,
                        i.EstimateItem!.WorkType!.Name,
                        i.EstimateItem.MeasurementUnit!.Code,
                        i.Volume,
                        i.UnitPrice,
                        i.TotalAmount)),
                    a.Payments.Select(p => new ClientActPaymentDto(
                        p.Id,
                        p.Date,
                        p.Amount,
                        p.PaymentMethod,
                        p.Note)),
                    a.Payments.Sum(p => p.Amount)))
                .FirstOrDefaultAsync(ct);

            return (act, false, false);
        }
    }
}
