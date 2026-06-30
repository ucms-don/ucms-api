using Ucms.Api.Extensions;

// Npgsql v6+ requires DateTimeOffset values to be in UTC when writing to timestamptz columns.
// This switch restores the legacy behavior that accepts any DateTimeOffset offset.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Console.Title = "UCMS API";

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder.Host.UseApplicationSerilog();

var app = builder
    .ConfigureServices()
    .ConfigurePipeline();

app.Run();
