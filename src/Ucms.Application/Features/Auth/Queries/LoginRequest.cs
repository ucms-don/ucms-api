namespace Ucms.Application.Features.Auth.Queries;

public record LoginRequest(
    string UserName,
    string Password
);
