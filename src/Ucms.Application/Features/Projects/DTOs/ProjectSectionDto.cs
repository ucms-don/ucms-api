namespace Ucms.Application.Features.Projects.DTOs;

public record ProjectSectionDto(
    Guid                              Id,
    string                            Name,
    int                               Order,
    IEnumerable<ProjectEstimateItemDto> Items);
