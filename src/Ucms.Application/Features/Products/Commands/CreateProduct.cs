namespace Ucms.Application.Features.Products.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateProduct
{
    public record Command(
        string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, string? InternationalCode, string? InternationalName,
        string? AlternativeName, ProductType Type);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (cmd.Code is not null && await db.Products.AnyAsync(f => f.Code == cmd.Code, ct))
                return (null, $"'{cmd.Code}' kodi allaqachon mavjud");

            var product = new Product
            {
                Name = cmd.Name, NameRu = cmd.NameRu, NameEn = cmd.NameEn, NameKa = cmd.NameKa,
                Code = cmd.Code, InternationalCode = cmd.InternationalCode,
                InternationalName = cmd.InternationalName, AlternativeName = cmd.AlternativeName,
                Type = cmd.Type
            };

            db.Products.Add(product);
            await db.SaveChangesAsync(ct);
            return (product.Id, null);
        }
    }
}
