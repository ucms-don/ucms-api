namespace Ucms.Application.Features.Salaries.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateSalary
{
    public record Command(Guid EmployeeId, string Month, decimal Amount, string? Notes, Guid CashAccountId);

    public record Result(Guid Id, string EmployeeName, decimal Amount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool EmployeeNotFound, bool Forbidden, bool CashAccountNotFound)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var employee = await db.Employees
                .Where(e => e.Id == cmd.EmployeeId && !e.IsDeleted)
                .Select(e => new { e.Id, e.Name, e.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (employee is null) return (null, true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != employee.OrganizationId) return (null, false, true, false);

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, employee.OrganizationId, ct))
                return (null, false, false, true);

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var salary = new Salary
            {
                Id             = Guid.NewGuid(),
                OrganizationId = employee.OrganizationId,
                EmployeeId     = cmd.EmployeeId,
                Month          = cmd.Month,
                Amount         = cmd.Amount,
                Notes          = cmd.Notes,
                IsDeleted      = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = userId, UpdatedBy = userId,
            };

            await db.Salaries.AddAsync(salary, ct);

            var date = DateTimeOffset.TryParse(cmd.Month + "-01", out var parsedDate) ? parsedDate : now;
            await CashTransactionLinker.UpsertAsync(
                db, CashTransactionSourceType.Salary, salary.Id,
                employee.OrganizationId, cmd.CashAccountId,
                CashDirection.Out, CashTransactionType.SalaryPayment,
                FinancePartnerType.Employee, cmd.EmployeeId,
                cmd.Amount, date, null, cmd.Notes,
                userId, ct);

            await db.SaveChangesAsync(ct);
            return (new Result(salary.Id, employee.Name, salary.Amount), false, false, false);
        }
    }
}
