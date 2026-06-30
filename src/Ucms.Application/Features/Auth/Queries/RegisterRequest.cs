namespace Ucms.Application.Features.Auth.Queries;

public record RegisterRequest(
    string UserName,
    string Email,
    string Password,
    string? FullName = null,
    Guid? OrganizationId = null
);
