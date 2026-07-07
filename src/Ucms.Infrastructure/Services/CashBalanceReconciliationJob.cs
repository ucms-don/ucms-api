namespace Ucms.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ucms.Application.Features.CashAccounts;

/// <summary>
/// Har kecha soat 02:00 da ICashBalanceReconciliationService ni chaqiradi.
/// </summary>
public sealed class CashBalanceReconciliationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<CashBalanceReconciliationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CashBalanceReconciliationJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextRun(TimeOnly.FromTimeSpan(TimeSpan.FromHours(2))); // 02:00
            logger.LogDebug("Next balance reconciliation in {Delay:hh\\:mm\\:ss}.", delay);

            try { await Task.Delay(delay, stoppingToken); }
            catch (OperationCanceledException) { break; }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var svc = scope.ServiceProvider
                    .GetRequiredService<ICashBalanceReconciliationService>();
                await svc.RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "CashBalanceReconciliationJob failed.");
            }
        }

        logger.LogInformation("CashBalanceReconciliationJob stopped.");
    }

    private static TimeSpan TimeUntilNextRun(TimeOnly targetTime)
    {
        var now  = DateTimeOffset.Now;
        var next = new DateTimeOffset(now.Date, now.Offset).Add(targetTime.ToTimeSpan());
        if (next <= now) next = next.AddDays(1);
        return next - now;
    }
}
