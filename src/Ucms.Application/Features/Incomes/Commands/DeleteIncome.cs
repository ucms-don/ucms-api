namespace Ucms.Application.Features.Incomes.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class DeleteIncome
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct)
        {
            var income = await db.Incomes.Include(i => i.IncomeItems)
                .AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (income is null) return false;
            foreach (var item in income.IncomeItems) item.IsDeleted = true;
            income.IsDeleted = true;
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
