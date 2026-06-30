namespace Ucms.Application.Features.Payments.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetFinancialSummary
{
    public record Query(Guid ProjectId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(object? Data, bool ProjectNotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => (Guid?)p.OrganizationId)
                .FirstOrDefaultAsync(ct);

            if (orgId is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != orgId) return (null, false, true);

            var clientReceived = await db.ClientPayments
                .Where(p => p.ProjectId == q.ProjectId).SumAsync(p => p.Amount, ct);

            var brigadePaid = await db.BrigadePayments
                .Where(p => p.ProjectId == q.ProjectId).SumAsync(p => p.Amount, ct);

            var estimateClientTotal = await db.Projects
                .Where(p => p.Id == q.ProjectId)
                .SelectMany(p => p.Estimates.SelectMany(a => a.Sections))
                .SelectMany(s => s.EstimateItems)
                .SumAsync(i => Math.Round(i.Volume * i.ClientUnitPrice, 2), ct);

            var estimateBrigadeTotal = await db.Projects
                .Where(p => p.Id == q.ProjectId)
                .SelectMany(p => p.Estimates.SelectMany(a => a.Sections))
                .SelectMany(s => s.EstimateItems)
                .SumAsync(i => Math.Round(i.Volume * i.BrigadeUnitPrice, 2), ct);

            var workedBrigadeTotal = await db.WorkLogs
                .Where(w => w.ProjectId == q.ProjectId).SumAsync(w => w.TotalAmount, ct);

            return (new
            {
                EstimateClientTotal  = estimateClientTotal,
                EstimateBrigadeTotal = estimateBrigadeTotal,
                EstimateProfit       = estimateClientTotal - estimateBrigadeTotal,

                ClientReceived   = clientReceived,
                ClientDebt       = estimateClientTotal - clientReceived,

                WorkedBrigadeTotal = workedBrigadeTotal,
                BrigadePaid        = brigadePaid,
                BrigadeDebt        = workedBrigadeTotal - brigadePaid,

                NetBalance = clientReceived - brigadePaid,
            }, false, false);
        }
    }
}
