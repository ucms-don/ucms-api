namespace Ucms.Application.Features.Skus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetSkuById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<SkuModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var sku = await db.Skus.FirstOrDefaultAsync(f => f.Id == q.Id, ct);
            if (sku is null) return null;

            var model = mapper.Map<SkuModel>(sku);
            model.CashAccountId = await db.CashTransactions
                .Where(t => t.SourceType == CashTransactionSourceType.SkuPurchase
                    && t.SourceId == sku.Id && !t.IsDeleted)
                .Select(t => (Guid?)t.CashAccountId)
                .FirstOrDefaultAsync(ct);
            return model;
        }
    }
}
