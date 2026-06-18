namespace Ucms.Application.Features.Salaries.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateSalary
{
    public record Command(Guid Id, Guid EmployeeId, string Month, decimal Amount, string? Notes, Guid CashAccountId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, string? Error, bool CashAccountNotFound)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var salary = await db.Salaries.FindAsync([cmd.Id], ct);
            if (salary is null || salary.IsDeleted) return (true, false, null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != salary.OrganizationId) return (false, true, null, false);

            // Yangi xodim tekshirish (agar o'zgartirilsa)
            if (salary.EmployeeId != cmd.EmployeeId)
            {
                var exists = await db.Employees
                    .AnyAsync(e => e.Id == cmd.EmployeeId && !e.IsDeleted, ct);
                if (!exists) return (false, false, "Xodim topilmadi", false);
            }

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, salary.OrganizationId, ct))
                return (false, false, null, true);

            salary.EmployeeId = cmd.EmployeeId;
            salary.Month      = cmd.Month;
            salary.Amount     = cmd.Amount;
            salary.Notes      = cmd.Notes;
            salary.UpdatedAt  = DateTimeOffset.UtcNow;
            salary.UpdatedBy  = ctx.UserId ?? Guid.Empty;

            db.Salaries.Update(salary);

            var userId = ctx.UserId ?? Guid.Empty;
            var date = DateTimeOffset.TryParse(cmd.Month + "-01", out var parsed) ? parsed : DateTimeOffset.UtcNow;
            await CashTransactionLinker.UpsertAsync(
                db, CashTransactionSourceType.Salary, salary.Id,
                salary.OrganizationId, cmd.CashAccountId,
                CashDirection.Out, CashTransactionType.SalaryPayment,
                FinancePartnerType.Employee, cmd.EmployeeId,
                cmd.Amount, date, null, cmd.Notes,
                userId, ct);

            await db.SaveChangesAsync(ct);
            return (false, false, null, false);
        }
    }
}
