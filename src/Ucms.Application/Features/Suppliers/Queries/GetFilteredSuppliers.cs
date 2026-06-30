namespace Ucms.Application.Features.Suppliers.Queries;

using AutoMapper;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Features.Suppliers.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetFilteredSuppliers
{
    public record Query(PagedRequest Filter);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<PagedResult<SupplierModel>> HandleAsync(Query q, CancellationToken ct)
        {
            return await db.Suppliers.OrderBy(x => x.Name)
                .ToPagedResultAsync<Supplier, SupplierModel>(q.Filter, mapper, ct);
        }
    }
}
