namespace Ucms.Api.Extensions;

using Serilog;

public static class LoggingConfiguration
{
    public static IHostBuilder UseApplicationSerilog(this IHostBuilder builder)
    {
        builder.UseSerilog((ctx, conf) => conf
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("EnvironmentUserName", Environment.UserName)
            .Enrich.WithProperty("ApplicationName", ctx.HostingEnvironment.ApplicationName)
            .Enrich.WithProperty("Env", ctx.HostingEnvironment.EnvironmentName));

        return builder;
    }
}
