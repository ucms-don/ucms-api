namespace Ucms.Application.Features.StockSkus.Queries;

using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Application.Abstractions.Organization;
using Ucms.Application.Features.StockSkus.DTOs;

public static class GetStockSkuStats
{
    public record Query(Guid OrganizationId);

    public sealed class Handler(IUcmsDbContext db, IOrganizationClient organizationClient)
    {
        public async Task<StockSkuStatModel> HandleAsync(Query q, CancellationToken ct)
        {
            var orgIds = await organizationClient.GetOrganizationIds(q.OrganizationId);
            var base_ = db.StockSkus.Where(w => orgIds.Contains(w.Stock!.OrganizationId));
            var car   = base_.Where(w => w.Stock!.StockType == StockType.Car).Sum(s => s.Amount);
            var case_ = base_.Where(w => w.Stock!.StockType == StockType.Case).Sum(s => s.Amount);
            var other = base_.Where(w => w.Stock!.StockType != StockType.Car && w.Stock!.StockType != StockType.Case).Sum(s => s.Amount);
            return new StockSkuStatModel(car, case_, other);
        }
    }
}
