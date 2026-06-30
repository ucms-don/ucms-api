namespace Ucms.Application.Features.Dashboard.DTOs;

public record DashboardFinanceDto(
    decimal ClientReceived,
    decimal BrigadePaid,
    decimal WorkedTotal,
    decimal BrigadeDebt);
