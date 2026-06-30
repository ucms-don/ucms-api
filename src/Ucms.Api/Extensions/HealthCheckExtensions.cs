namespace Ucms.Api.Extensions;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck(
                name: "self",
                check: () => HealthCheckResult.Healthy(),
                tags: ["ready", "live"]);

        return services;
    }

    public static IEndpointRouteBuilder MapApplicationHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready")
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return endpoints;
    }
}
