namespace Ucms.Application.Services;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

/// <summary>
/// Code'ni "WT-000001" ko'rinishida generatsiya qiladi. Mavjud WorkType'lar orasidan
/// shu prefiks bilan boshlanuvchi eng katta tartib raqamini topib, undan keyingisini qaytaradi.
/// </summary>
public sealed class WorkTypeCodeGenerator(IUcmsDbContext db) : IWorkTypeCodeGenerator
{
    private const string Prefix = "WT-";

    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var existingCodes = await db.WorkTypes
            .IgnoreQueryFilters()
            .Where(w => w.Code != null && w.Code.StartsWith(Prefix))
            .Select(w => w.Code!)
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
        // Bir vaqtda bir nechta so'rov kelishi mumkin bo'lgani uchun, noyoblikni
        // yana bir bor tekshirib, to'qnashuv bo'lsa keyingi raqamga o'tamiz.
        do
        {
            candidate = $"{Prefix}{next:D6}";
            next++;
        }
        while (await db.WorkTypes.IgnoreQueryFilters().AnyAsync(w => w.Code == candidate, ct));

        return candidate;
    }
}
