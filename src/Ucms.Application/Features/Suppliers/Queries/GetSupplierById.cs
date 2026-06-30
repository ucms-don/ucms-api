namespace Ucms.Application.Features.Suppliers.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Suppliers.DTOs;
using Ucms.Application.Persistence;

public static class GetSupplierById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<SupplierModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var entity = await db.Suppliers.FirstOrDefaultAsync(f => f.Id == q.Id, ct);
            return entity is null ? null : mapper.Map<SupplierModel>(entity);
        }
    }
}
