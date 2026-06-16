namespace Ucms.Application.Features.Projects.DTOs;

using Ucms.Domain.Enums;

public record ProjectDetailDto(
    Guid                           Id,
    string                         Name,
    string?                        ClientName,
    Guid?                          CustomerId,
    string?                        CustomerName,
    string?                        Address,
    string?                        Description,
    string?                        ContractNumber,
    DateTimeOffset?                ContractDate,
    decimal?                       ContractValue,
    DateTimeOffset?                StartDate,
    DateTimeOffset?                EndDate,
    ProjectStatus                  Status,
    string                         StatusString,
    Guid                           OrganizationId,
    DateTimeOffset                 CreatedAt,
    DateTimeOffset                 UpdatedAt,
    IEnumerable<ProjectSectionDto> Sections);
