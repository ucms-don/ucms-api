namespace Ucms.Application.Features.Products.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Products.DTOs;
using Ucms.Application.Persistence;

public static class GetProductById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<ProductModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var product = await db.Products.FirstOrDefaultAsync(f => f.Id == q.Id, ct);
            return product is null ? null : mapper.Map<ProductModel>(product);
        }
    }
}
