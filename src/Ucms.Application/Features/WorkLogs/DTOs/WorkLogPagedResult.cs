namespace Ucms.Application.Features.WorkLogs.DTOs;

public record WorkLogPagedResult(
    int              Total,
    int              Page,
    int              Size,
    List<WorkLogDto> Items);
