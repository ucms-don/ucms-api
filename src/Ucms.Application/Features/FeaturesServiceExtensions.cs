namespace Ucms.Application.Features;

using Microsoft.Extensions.DependencyInjection;
using Ucms.Application.Services;

public static class FeaturesServiceExtensions
{
    /// <summary>
    /// Features papkasidagi barcha Handler classlarni Scoped sifatida ro'yxatdan o'tkazadi.
    /// </summary>
    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        var assembly = typeof(FeaturesServiceExtensions).Assembly;

        var handlers = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name == "Handler"
                     && t.IsNested
                     && t.DeclaringType?.Namespace?.StartsWith("Ucms.Application.Features") == true);

        foreach (var handler in handlers)
            services.AddScoped(handler);

        // Application services
        services.AddScoped<IIncomeService, IncomeService>();
        services.AddScoped<IOutcomeService, OutcomeService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IProductBalanceReportService, ProductBalanceReportService>();
        services.AddScoped<ISkuSerialNumberGenerator, SkuSerialNumberGenerator>();
        services.AddScoped<IWorkTypeCodeGenerator, WorkTypeCodeGenerator>();

        return services;
    }
}
