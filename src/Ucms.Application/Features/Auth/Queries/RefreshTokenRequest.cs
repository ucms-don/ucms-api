namespace Ucms.Application.Features.Auth.Queries;

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
