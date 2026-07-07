namespace Ucms.Application.Features.Salaries.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

public static class CreateSalary
{
    public record Command(Guid EmployeeId, string Month, decimal Amount, string? Notes, Guid CashAccountId);

    public record Result(Guid Id, string EmployeeName, decimal Amount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx, ICashBalanceService balanceService)
    {
        public async Task<(Result? Data, bool EmployeeNotFound, bool Forbidden, bool CashAccountNotFound, bool InsufficientBalance)>
            HandleAsync(Command cmd, CancellationToken ct)
        {
            var employee = await db.Employees
                .Where(e => e.Id == cmd.EmployeeId)
                .Select(e => new { e.Id, e.Name, e.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (employee is null) return (null, true, false, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != employee.OrganizationId) return (null, false, true, false, false);

            if (!await CashTransactionLinker.CashAccountExistsAsync(db, cmd.CashAccountId, employee.OrganizationId, ct))
                return (null, false, false, true, false);

            var salaryId = Guid.NewGuid();
            var userId   = ctx.UserId ?? Guid.Empty;
            var orgId    = employee.OrganizationId;
            var date     = DateTimeOffset.TryParse(cmd.Month + "-01", out var parsedDate) ? parsedDate : DateTimeOffset.UtcNow;

            try
            {
                await db.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ClearChangeTracker();
                    await using var tx = await db.BeginTransactionAsync(ct);

                    var now = DateTimeOffset.UtcNow;
                    var salary = new Salary
                    {
                        Id             = salaryId,
                        OrganizationId = orgId,
                        EmployeeId     = cmd.EmployeeId,
                        Month          = cmd.Month,
                        Amount         = cmd.Amount,
                        Notes          = cmd.Notes,
                        IsDeleted      = false,
                        CreatedAt      = now, UpdatedAt = now,
                        CreatedBy      = userId, UpdatedBy = userId,
                    };
                    await db.Salaries.AddAsync(salary, ct);

                    await CashTransactionLinker.UpsertAsync(
                        db, balanceService,
                        CashTransactionSourceType.Salary, salaryId,
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
                return (null, false, false, false, true);
            }

            return (new Result(salaryId, employee.Name, cmd.Amount), false, false, false, false);
        }
    }
}
