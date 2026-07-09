namespace Ucms.Application.Services;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

/// <summary>
/// Code'ni "STK-000001" ko'rinishida generatsiya qiladi.
/// Mavjud Stock'lar orasidan shu prefiks bilan boshlanuvchi
/// eng katta tartib raqamini topib, undan keyingisini qaytaradi.
/// </summary>
public sealed class StockCodeGenerator(IUcmsDbContext db) : IStockCodeGenerator
{
    private const string Prefix = "STK-";

    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var existingCodes = await db.Stocks
            .IgnoreQueryFilters()
            .Where(s => s.Code != null && s.Code.StartsWith(Prefix))
            .Select(s => s.Code!)
            .ToListAsync(ct);

        var maxNumber = 0;
        foreach (var code in existingCodes)
        {
            if (code.Length <= Prefix.Length) continue;
            var tail = code[Prefix.Length..];
            if (int.TryParse(tail, out var n) && n > maxNumber) maxNumber = n;
        }

        var next = maxNumber + 1;
        string candidate;
        do
        {
            candidate = $"{Prefix}{next:D6}";
            next++;
        }
        while (await db.Stocks.IgnoreQueryFilters().AnyAsync(s => s.Code == candidate, ct));

        return candidate;
    }
}
