namespace Ucms.Application.Features.Dashboard.DTOs;

public record DashboardProjectStatsDto(
    int Total,
    int Planning,
    int InProgress,
    int Completed,
    int Suspended);
