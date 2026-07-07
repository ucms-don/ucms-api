namespace Ucms.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ucms.Application.Features.CashTransactions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;
using Ucms.Domain.Exceptions;

/// <summary>
/// apply_cash_balance_delta() PostgreSQL SP ni chaqiradi.
/// SP ichida SELECT ... FOR UPDATE orqali CashAccount qatori lock qilinadi,
/// balans atomik yangilanadi, va InsufficientBalance / AccountNotFound holatlari
/// PostgreSQL ERRCODE orqali xatolik sifatida qaytariladi.
/// </summary>
public class CashBalanceService(IUcmsDbContext db) : ICashBalanceService
{
    public async Task<decimal> ApplyDeltaAsync(
        Guid accountId,
        decimal amount,
        CashDirection direction,
        bool allowOverdraft = false,
        CancellationToken ct = default)
    {
        try
        {
            var dir = (int)direction; // CashDirection.In = 1, CashDirection.Out = 2

            var newBalance = await db.Database
                .SqlQuery<decimal>(
                    $"SELECT apply_cash_balance_delta({accountId}, {amount}, {dir}, {allowOverdraft}) AS \"Value\"")
                .FirstAsync(ct);

            return newBalance;
        }
        catch (PostgresException ex) when (ex.SqlState == "P0002")
        {
            throw new InsufficientBalanceException(ex.MessageText, ex);
        }
        catch (PostgresException ex) when (ex.SqlState == "P0001")
        {
            throw new CashAccountNotFoundException(ex.MessageText, ex);
        }
        catch (PostgresException ex) when (ex.SqlState == "P0003")
        {
            // p_amount <= 0: dastur xatosi, 500 sifatida qaytariladi
            throw new InvalidOperationException($"apply_cash_balance_delta: {ex.MessageText}", ex);
        }
    }
}
