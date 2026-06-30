namespace Ucms.Api.Extensions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Auth;
using Ucms.Application.Abstractions.Authorization;
using Ucms.Application.Abstractions.Organization;
using Ucms.Application.Abstractions.Storage;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities.Identity;
using Ucms.Infrastructure.Persistence;
using Ucms.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUcmsDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<UcmsDbContext>(options =>
        {
            options.UseNpgsql(connectionString,
                    optionsBuilder =>
                    {
                        optionsBuilder.MigrationsAssembly(typeof(UcmsDbContext).Assembly.GetName().Name);
                        optionsBuilder.EnableRetryOnFailure(maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                    })
                .EnableSensitiveDataLogging()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, ServiceLifetime.Scoped);

        services.AddScoped<IUcmsDbContext, UcmsDbContext>();

        return services;
    }

    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<User, Role>(options =>
            {
                // Parol talablari
                options.Password.RequireDigit           = true;
                options.Password.RequiredLength         = 8;
                options.Password.RequireUppercase       = false;
                options.Password.RequireNonAlphanumeric = false;

                // Login bloklash
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);

                // Foydalanuvchi sozlamalari
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UcmsDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddUcmsTokenService(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }

    public static IServiceCollection AddUcmsInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IWorkContext, HttpWorkContext>();
        services.AddScoped<IOrganizationClient, StubOrganizationClient>();
        services.AddScoped<IPermissionProvider, StubPermissionProvider>();
        services.AddScoped<IFileStorageClient, StubFileStorageClient>();
        services.AddScoped<IAvatarStorageClient, LocalAvatarStorageClient>();
        return services;
    }

    public static IServiceCollection AddUcmsCors(this IServiceCollection services, string policyName)
    {
        services.AddCors(builder =>
        {
            builder.AddPolicy(policyName, options =>
                options
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }
}
