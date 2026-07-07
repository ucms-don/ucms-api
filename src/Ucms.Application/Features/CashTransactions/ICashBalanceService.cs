namespace Ucms.Application.Features.CashTransactions;

using Ucms.Domain.Enums;

/// <summary>
/// CashAccount balansini apply_cash_balance_delta() PostgreSQL SP orqali atomik o'zgartiradi.
/// Barcha chaqiruvlar ochiq DB tranzaksiyasi ichida bo'lishi shart — SP FOR UPDATE lock ishlatadi.
/// </summary>
public interface ICashBalanceService
{
    /// <summary>
    /// Belgilangan hisobning balansiga delta qo'shadi (kirim) yoki ayiradi (chiqim).
    /// </summary>
    /// <param name="accountId">CashAccount.Id</param>
    /// <param name="amount">Har doim musbat summa</param>
    /// <param name="direction">CashDirection.In yoki CashDirection.Out</param>
    /// <param name="allowOverdraft">true — manfiy balansga ruxsat (masalan, reversal operatsiyalar uchun)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Yangi balans qiymati</returns>
    public Task<decimal> ApplyDeltaAsync(
        Guid accountId,
        decimal amount,
        CashDirection direction,
        bool allowOverdraft = false,
        CancellationToken ct = default);
}
