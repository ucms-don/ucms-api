namespace Ucms.Application.Services;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

/// <summary>
/// SerialNumber'ni Product.Code asosida generatsiya qiladi: "{PRODUCT_CODE}-000001" ko'rinishida.
/// Mahsulotda Code bo'lmasa, umumiy "SN-000001" prefiksi ishlatiladi. Mavjud Sku'lar orasidan
/// shu prefiks bilan boshlanuvchi eng katta tartib raqamini topib, undan keyingisini qaytaradi.
/// </summary>
public sealed class SkuSerialNumberGenerator(IUcmsDbContext db) : ISkuSerialNumberGenerator
{
    private const string DefaultPrefix = "SN";

    public async Task<string> GenerateAsync(Guid productId, CancellationToken ct = default)
    {
        var productCode = await db.Products
            .Where(p => p.Id == productId)
            .Select(p => p.Code)
            .FirstOrDefaultAsync(ct);

        var prefix = BuildPrefix(productCode);

        var existingSerials = await db.Skus
            .Where(s => s.SerialNumber.StartsWith(prefix))
            .Select(s => s.SerialNumber)
            .ToListAsync(ct);

        var maxNumber = 0;
        foreach (var serial in existingSerials)
        {
            if (serial.Length <= prefix.Length) continue;
            var tail = serial[prefix.Length..];
            if (int.TryParse(tail, out var n) && n > maxNumber) maxNumber = n;
        }

        var next = maxNumber + 1;
        string candidate;
        // Bir vaqtda bir nechta so'rov kelishi mumkin bo'lgani uchun, noyoblikni
        // yana bir bor tekshirib, to'qnashuv bo'lsa keyingi raqamga o'tamiz.
        do
        {
            candidate = $"{prefix}{next:D6}";
            next++;
        }
        while (await db.Skus.AnyAsync(s => s.SerialNumber == candidate, ct));

        return candidate;
    }

    private static string BuildPrefix(string? productCode)
    {
        var cleaned = string.IsNullOrWhiteSpace(productCode)
            ? DefaultPrefix
            : new string(productCode.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        if (string.IsNullOrEmpty(cleaned)) cleaned = DefaultPrefix;
        return $"{cleaned}-";
    }
}
