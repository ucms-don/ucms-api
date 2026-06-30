namespace Ucms.Api.Extensions;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class PermissionsConfiguration
{
    public static IServiceCollection AddApplicationAuth(this IServiceCollection services, IConfiguration config)
    {
        var key = Encoding.UTF8.GetBytes(
            config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key sozlanmagan"));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = config["Jwt:Issuer"],
                    ValidAudience            = config["Jwt:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ClockSkew                = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
