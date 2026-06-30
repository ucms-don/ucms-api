namespace Ucms.Api.Extensions;

using System.Text.Json;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Ucms.Api.Middlewares;
using Ucms.Application.Abstractions;
using Ucms.Application.Features;
using Ucms.Application.Features.MeasurementUnits.MappingProfiles;
using Ucms.Infrastructure.Persistence;
using Ucms.Infrastructure.Services;

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
        builder.Services.AddScoped<ICurrentContext, HttpCurrentContext>();
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

        // Model-validation xatolari HAR DOIM ikki tilda (o'zbek + rus) qaytsin.
        // Ошибки валидации модели ВСЕГДА возвращаются на двух языках (узбекский + русский).
        builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var details = context.ModelState
                    .Where(kvp => kvp.Value is not null && kvp.Value.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct()
                    .ToList();

                var message = "Ma'lumotlar noto'g'ri to'ldirilgan. / Введены некорректные данные.";
                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { message, errors = details });
            };
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

        var avatarsDir = AvatarPathResolver.Resolve(app.Configuration, app.Environment);
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
