namespace Ucms.Application.Features.Products.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateProduct
{
    public record Command(
        Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, string? InternationalCode, string? InternationalName,
        string? AlternativeName, ProductType Type);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var product = await db.Products.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (product is null) return false;

            product.Name = cmd.Name; product.NameRu = cmd.NameRu;
            product.NameEn = cmd.NameEn; product.NameKa = cmd.NameKa;
            product.Code = cmd.Code; product.InternationalCode = cmd.InternationalCode;
            product.InternationalName = cmd.InternationalName;
            product.AlternativeName = cmd.AlternativeName;
            product.Type = cmd.Type;

            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
