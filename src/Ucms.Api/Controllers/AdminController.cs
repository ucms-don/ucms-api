namespace Ucms.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ucms.Application.Features.CashAccounts;

/// <summary>
/// Admin uchun yordamchi operatsiyalar (balans tekshirish va boshqalar).
/// Вспомогательные операции для администратора.
/// </summary>
[ApiController]
[Route("api/admin")]
[Tags("Admin")]
[Authorize(Roles = "Admin")]
public class AdminController(ICashBalanceReconciliationService reconciliationService) : ControllerBase
{
    /// <summary>
    /// Barcha kassalarning balansini tekshiradi va tuzatadi.
    /// Проверяет и корректирует балансы всех касс.
    /// </summary>
    [HttpPost("cash-balance/reconcile")]
    [ProducesResponseType(typeof(ReconciliationResultDto), 200)]
    public async Task<IActionResult> ReconcileBalances(CancellationToken ct)
    {
        var result = await reconciliationService.RunAsync(ct);

        return Ok(new ReconciliationResultDto(
            result.TotalAccounts,
            result.FixedAccounts,
            result.Mismatches.Select(m => new MismatchDto(
                m.AccountId,
                m.AccountName,
                m.StoredBalance,
                m.RealBalance,
                m.Diff)).ToList()));
    }

    public record ReconciliationResultDto(
        int TotalAccounts,
        int FixedAccounts,
        IReadOnlyList<MismatchDto> Mismatches);

    public record MismatchDto(
        Guid AccountId,
        string AccountName,
        decimal StoredBalance,
        decimal RealBalance,
        decimal Diff);
}
