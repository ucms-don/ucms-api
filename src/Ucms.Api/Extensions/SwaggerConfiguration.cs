namespace Ucms.Api.Extensions;

using System.Reflection;
using Microsoft.OpenApi.Models;
using Ucms.Api.Filters;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddUcmsSwagger(this IServiceCollection services, IConfiguration config)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version     = "v1",
                Title       = "UCMS API",
                Description = "Unified Construction Management System — REST API",
            });

            options.OperationFilter<SwaggerOperationIdFilter>();

            // JWT Bearer autentifikatsiya
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.Http,
                Scheme      = "bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "JWT tokenni kiriting. Token olish uchun **Login sahifasini** oching.",
            });

            options.OperationFilter<BearerSecurityOperationFilter>();

            // XML doc commentlarni Swagger'ga ulash
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
