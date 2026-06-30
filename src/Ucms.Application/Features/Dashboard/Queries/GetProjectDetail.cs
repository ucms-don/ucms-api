namespace Ucms.Application.Features.Dashboard.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetProjectDetail
{
    public record Query(Guid ProjectId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(object? Data, bool NotFound, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var project = await db.Projects
                .Where(p => p.Id == q.ProjectId && !p.IsDeleted)
                .Select(p => new { p.Name, p.Status, p.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (project is null) return (null, true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != project.OrganizationId) return (null, false, true);

            var estimateTotals = await db.EstimateItems
                .Where(i => i.Section!.Estimate!.ProjectId == q.ProjectId)
                .GroupBy(_ => true)
                .Select(g => new
                {
                    ClientTotal  = Math.Round(g.Sum(i => i.Volume * i.ClientUnitPrice),  2),
                    BrigadeTotal = Math.Round(g.Sum(i => i.Volume * i.BrigadeUnitPrice), 2),
                })
                .FirstOrDefaultAsync(ct)
                ?? new { ClientTotal = 0m, BrigadeTotal = 0m };

            var workStats = await db.WorkLogs
                .Where(w => w.ProjectId == q.ProjectId)
                .GroupBy(_ => true)
                .Select(g => new
                {
                    Worked    = g.Sum(w => w.TotalAmount),
                    Confirmed = g.Where(w => w.Status == WorkLogStatus.Confirmed).Sum(w => w.TotalAmount),
                    Paid      = g.Where(w => w.Status == WorkLogStatus.Paid).Sum(w => w.TotalAmount),
                })
                .FirstOrDefaultAsync(ct)
                ?? new { Worked = 0m, Confirmed = 0m, Paid = 0m };

            var actStats = await db.ClientActs
                .Where(a => a.ProjectId == q.ProjectId)
                .GroupBy(_ => true)
                .Select(g => new
                {
                    Total       = g.Count(),
                    TotalAmount = g.Sum(a => a.TotalAmount),
                    Issued      = g.Count(a => a.Status == ActStatus.Issued),
                    PaidFully   = g.Count(a => a.Status == ActStatus.PaidFully),
                })
                .FirstOrDefaultAsync(ct)
                ?? new { Total = 0, TotalAmount = 0m, Issued = 0, PaidFully = 0 };

            var clientReceived = await db.ClientPayments
                .Where(p => p.ProjectId == q.ProjectId).SumAsync(p => p.Amount, ct);

            var brigadePaid = await db.BrigadePayments
                .Where(p => p.ProjectId == q.ProjectId).SumAsync(p => p.Amount, ct);

            var brigadeBreakdown = await db.WorkLogs
                .Where(w => w.ProjectId == q.ProjectId)
                .GroupBy(w => w.BrigadeId)
                .Select(g => (object)new
                {
                    BrigadeId   = g.Key,
                    BrigadeName = g.First().Brigade!.Name,
                    TotalWorked = g.Sum(w => w.TotalAmount),
                    TotalPaid   = g.Where(w => w.Status == WorkLogStatus.Paid).Sum(w => w.TotalAmount),
                    Debt        = g.Where(w => w.Status != WorkLogStatus.Paid).Sum(w => w.TotalAmount),
                })
                .ToListAsync(ct);

            return (new
            {
                Project  = new { project.Name, project.Status },
                Estimate = new
                {
                    estimateTotals.ClientTotal,
                    estimateTotals.BrigadeTotal,
                    Profit = estimateTotals.ClientTotal - estimateTotals.BrigadeTotal,
                },
                Work = workStats,
                Acts = actStats,
                Finance = new
                {
                    ClientTotal    = estimateTotals.ClientTotal,
                    ClientReceived = clientReceived,
                    ClientDebt     = estimateTotals.ClientTotal - clientReceived,
                    BrigadeTotal   = workStats.Worked,
                    BrigadePaid    = brigadePaid,
                    BrigadeDebt    = workStats.Worked - brigadePaid,
                    NetBalance     = clientReceived - brigadePaid,
                },
                BrigadeBreakdown = brigadeBreakdown,
            }, false, false);
        }
    }
}
