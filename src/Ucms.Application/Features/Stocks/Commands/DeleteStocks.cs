namespace Ucms.Application.Features.Stocks.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteStocks
{
    public record Command(Guid[] Ids);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext)
    {
        public async Task<(int Deleted, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (db.Incomes.Any(a => cmd.Ids.Contains(a.StockId)) || db.Outcomes.Any(a => cmd.Ids.Contains(a.StockId)) ||
                db.StockSkus.Any(a => cmd.Ids.Contains(a.StockId)) || db.Stocks.Any(a => cmd.Ids.Contains(a.ParentId ?? Guid.Empty)))
                return (0, "Omborlar boshqa jadvallarda ishlatilmoqda");

            var query = db.Stocks.AsTracking().Where(f => cmd.Ids.Contains(f.Id));
            if (!workContext.IsAdmin) query = query.Where(f => f.OrganizationId == workContext.TenantId);
            var stocks = await query.ToListAsync(ct);
            foreach (var s in stocks) s.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (stocks.Count, null);
        }
    }
}
