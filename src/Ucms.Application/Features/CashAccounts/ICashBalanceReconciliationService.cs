namespace Ucms.Application.Features.CashAccounts;

public interface ICashBalanceReconciliationService
{
    /// <summary>
    /// Barcha CashAccount larning Balance ni CashTransactions yig'indisi bilan solishtiradi.
    /// Farq bo'lsa — to'g'rilaydi.
    /// </summary>
    Task<ReconciliationResult> RunAsync(CancellationToken ct = default);
}

public record ReconciliationResult(
    int TotalAccounts,
    int FixedAccounts,
    IReadOnlyList<ReconciliationMismatch> Mismatches);

public record ReconciliationMismatch(
    Guid AccountId,
    string AccountName,
    decimal StoredBalance,
    decimal RealBalance,
    decimal Diff);
