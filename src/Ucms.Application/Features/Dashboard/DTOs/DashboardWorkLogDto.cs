namespace Ucms.Application.Features.Dashboard.DTOs;

using Ucms.Domain.Enums;

public record DashboardWorkLogDto(
    Guid           Id,
    DateTimeOffset Date,
    decimal        TotalAmount,
    WorkLogStatus  Status,
    string         Project,
    string         Brigade,
    string         EstimateItem);
