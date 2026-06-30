namespace Ucms.Application.Features.Salaries.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class DeleteSalary
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var salary = await db.Salaries.FindAsync([cmd.Id], ct);
            if (salary is null || salary.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != salary.OrganizationId) return (false, true);

            salary.IsDeleted = true;
            salary.UpdatedAt = DateTimeOffset.UtcNow;
            salary.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Salaries.Update(salary);

            await CashTransactionLinker.RemoveAsync(db, CashTransactionSourceType.Salary, salary.Id, salary.UpdatedBy, ct);

            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
