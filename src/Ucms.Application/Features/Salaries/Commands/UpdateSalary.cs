namespace Ucms.Application.Features.Salaries.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class UpdateSalary
{
    public record Command(Guid Id, Guid EmployeeId, string Month, decimal Amount, string? Notes, Guid CashAccountId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(bool NotFound, bool Forbidden, string? Error, bool CashAccountNotFound, bool InsufficientBalance)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var salary = await db.Salaries.FindAsync([cmd.Id], ct);
            if (salary is null || salary.IsDeleted) return (true, false, null, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != salary.OrganizationId) return (false, true, null, false, false);

            if (salary.EmployeeId != cmd.EmployeeId)
            {
                var exists = await db.Employees.AnyAsync(e => e.Id == cmd.EmployeeId, ct);
                if (!exists) return (false, false, "Xodim topilmadi", false, false);
            }

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, salary.OrganizationId, ct))
                return (false, false, null, true, false);

            var userId = ctx.UserId ?? Guid.Empty;
            var orgId  = salary.OrganizationId;
            var date   = DateTimeOffset.TryParse(cmd.Month + "-01", out var parsed) ? parsed : DateTimeOffset.UtcNow;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var s = await db.Salaries.FindAsync([cmd.Id], ct);
                    if (s is null || s.IsDeleted) return;

                    s.EmployeeId = cmd.EmployeeId;
                    s.Month      = cmd.Month;
                    s.Amount     = cmd.Amount;
                    s.Notes      = cmd.Notes;
                    s.UpdatedAt  = DateTimeOffset.UtcNow;
                    s.UpdatedBy  = userId;
                    db.Salaries.Update(s);

                    await CashTransactionLinker.UpsertAsync(
                        db, balanceService,
                        CashTransactionSourceType.Salary, cmd.Id,
                        orgId, cmd.CashAccountId,
                        CashDirection.Out, CashTransactionType.SalaryPayment,
                        FinancePartnerType.Employee, cmd.EmployeeId,
                        cmd.Amount, date, null, cmd.Notes,
                        userId, ct);

                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                });
            }
            catch (InsufficientBalanceException)
            {
                return (false, false, null, false, true);
            }

            return (false, false, null, false, false);
        }
    }
}
