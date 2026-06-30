namespace Ucms.Application.Features.Dashboard.DTOs;

public record DashboardDto(
    DashboardProjectStatsDto      Projects,
    DashboardBrigadeStatsDto      Brigades,
    DashboardFinanceDto           Finance,
    IEnumerable<DashboardWorkLogDto> RecentWorkLogs,
    IEnumerable<DashboardPaymentDto> RecentPayments);
