namespace Ucms.Application.Features.Stocks.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Organization;
using Ucms.Application.Persistence;

public static class DeleteStock
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, IOrganizationClient organizationClient)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var stock = workContext.IsAdmin
                ? await db.Stocks.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.Id, ct)
                : await db.Stocks.AsTracking().FirstOrDefaultAsync(a => a.Id == cmd.Id && a.OrganizationId == workContext.TenantId, ct);
            if (stock is null) return (true, null);

            if (await organizationClient.CheckOrganizationBrigadeStock(stock.Id))
                return (false, "Ombor brigadaga biriktirilgan");

            if (db.Incomes.Any(a => a.StockId == cmd.Id) || db.Outcomes.Any(a => a.StockId == cmd.Id) ||
                db.StockSkus.Any(a => a.StockId == cmd.Id) || db.Stocks.Any(a => a.ParentId == cmd.Id))
                return (false, "Ombor boshqa jadvallarda ishlatilmoqda");

            stock.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
