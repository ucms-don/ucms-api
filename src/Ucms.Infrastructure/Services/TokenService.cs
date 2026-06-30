namespace Ucms.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Ucms.Application.Abstractions.Auth;
using Ucms.Domain.Entities.Identity;

public class TokenService(IConfiguration config) : ITokenService
{
    private readonly string _key       = config["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key mavjud emas");
    private readonly string _issuer    = config["Jwt:Issuer"]   ?? "ucms-api";
    private readonly string _audience  = config["Jwt:Audience"] ?? "ucms-clients";
    private readonly int    _accessMin = int.TryParse(config["Jwt:AccessTokenExpirationMinutes"] ?? "60", out var accessMin) ? accessMin : 60;
    private readonly int    _refreshDays = int.TryParse(config["Jwt:RefreshTokenExpirationDays"] ?? "7", out var refreshDays) ? refreshDays : 7;

    public string GenerateAccessToken(User user, IList<string> roles, string? orgType = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email   ?? ""),
            new(ClaimTypes.Name,               user.UserName ?? ""),
            new(ClaimTypes.NameIdentifier,     user.Id.ToString()),
        };

        if (user.OrganizationId.HasValue)
            claims.Add(new Claim("organization_id", user.OrganizationId.Value.ToString()));

        // Tashkilot turi — Owner yoki Tenant
        if (!string.IsNullOrWhiteSpace(orgType))
            claims.Add(new Claim("org_type", orgType));

        if (!string.IsNullOrWhiteSpace(user.FullName))
            claims.Add(new Claim("full_name", user.FullName));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            expires:            GetAccessTokenExpiry().UtcDateTime,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = false, // muddati o'tgan ham bo'lishi mumkin
            ValidateIssuerSigningKey = true,
            ValidIssuer              = _issuer,
            ValidAudience            = _audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public DateTimeOffset GetAccessTokenExpiry()
    {
        return DateTimeOffset.UtcNow.AddMinutes(_accessMin);
    }

    public DateTimeOffset GetRefreshTokenExpiry()
    {
        return DateTimeOffset.UtcNow.AddDays(_refreshDays);
    }
}
