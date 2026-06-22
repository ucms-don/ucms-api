namespace Ucms.Api.Extensions;

using System.Text.Json;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Ucms.Api.Middlewares;
using Ucms.Application.Features;
using Ucms.Application.Features.MeasurementUnits.MappingProfiles;
using Ucms.Infrastructure.Persistence;

public static class WebApplicationExtensions
{
    public static WebApplication MigrateDbContext<TContext>(
        this WebApplication app,
        Action<TContext, IServiceProvider> seeder)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        seeder(context, scope.ServiceProvider);
        return app;
    }

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
        builder.Services.AddUcmsDbContext(connectionString);
        builder.Services.AddAppIdentity();
        builder.Services.AddUcmsTokenService();
        builder.Services.AddUcmsCors("UcmsCors");
        builder.Services.AddApplicationAuth(builder.Configuration);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<Application.Abstractions.ICurrentContext, Infrastructure.Services.HttpCurrentContext>();
        builder.Services.AddUcmsInfrastructureServices();
        builder.Services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotationsValidation = true);
        builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MeasurementUnitProfile).Assembly));
        builder.Services.AddFeatureHandlers();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddUcmsSwagger(builder.Configuration);
        builder.Services.AddApplicationHealthChecks();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseMiddleware<GlobalMiddlewareErrorHander>();
        app.UseStaticFiles();

        var avatarsDir = Infrastructure.Services.AvatarPathResolver.Resolve(app.Configuration, app.Environment);
        Directory.CreateDirectory(avatarsDir);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(avatarsDir),
            RequestPath  = "/api/avatars",
        });
        app.UseRouting();
        app.UseCors("UcmsCors");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "UCMS API v1");
            c.DocumentTitle = "UCMS API";
            c.InjectJavascript("/swagger-custom.js");
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapApplicationHealthChecks();
        app.MapControllers();

        // Swagger uchun custom login sahifasi
        app.MapGet("/auth/login", (IWebHostEnvironment env) =>
            Results.File(
                Path.Combine(env.WebRootPath, "auth", "login.html"),
                "text/html"))
            .ExcludeFromDescription();

        app.MigrateDbContext<UcmsDbContext>((context, services) =>
        {
            context.Database.Migrate();

            var config = services.GetRequiredService<IConfiguration>();
            if (config.GetValue<bool>("Database:EnabledDataSeeding"))
            {
                new UcmsDbContextSeed()
                    .SeedAsync(services)
                    .Wait();
            }
        });

        return app;
    }
}
