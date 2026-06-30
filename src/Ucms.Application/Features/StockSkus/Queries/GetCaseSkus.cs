namespace Ucms.Application.Features.StockSkus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Features.StockSkus.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class GetCaseSkus
{
    public record Query(PagedRequest Paging, Guid OrganizationId, Guid StockId, string? Seria);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<(PagedResult<StockSkuModel>? Result, string? Error)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!db.Stocks.Any(a => a.Id == q.StockId && a.StockType == StockType.Case))
                return (null, "Ombor turi Case emas");

            var query = db.StockSkus
                .Include(i => i.Sku!.Product).Include(i => i.Stock).Include(i => i.MeasurementUnit)
                .Where(w => w.Stock!.OrganizationId == q.OrganizationId && w.StockId == q.StockId);

            if (!string.IsNullOrEmpty(q.Seria))
            {
                var s = q.Seria.ToLower();
                query = query.Where(w => w.Sku!.SerialNumber.ToLower().Contains(s));
            }

            var result = await query.OrderBy(a => a.Stock!.Name)
                .ToPagedResultAsync<StockSku, StockSkuModel>(q.Paging, mapper, ct);
            return (result, null);
        }
    }
}
