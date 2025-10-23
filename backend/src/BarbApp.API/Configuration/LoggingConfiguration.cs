using Serilog;
using Sentry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BarbApp.API.Configuration;

public static class LoggingConfiguration
{
    public static void ConfigureLogging(WebApplicationBuilder builder)
    {
        // Serilog configuration
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/barbapp-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static void ConfigureSentry(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.WebHost.UseSentry(options =>
            {
                string? Get(string key, string envKey)
                {
                    var v = builder.Configuration[key];
                    var isPlaceholder = !string.IsNullOrWhiteSpace(v) && v.TrimStart().StartsWith("${") && v.TrimEnd().EndsWith("}");
                    if (string.IsNullOrWhiteSpace(v) || isPlaceholder)
                        v = Environment.GetEnvironmentVariable(envKey);
                    return string.IsNullOrWhiteSpace(v) ? null : v;
                }

                options.Dsn = Get("Sentry:Dsn", "SENTRY_DSN");
                options.Environment = Get("Sentry:Environment", "SENTRY_ENVIRONMENT") ?? builder.Environment.EnvironmentName;
                options.Release = Get("Sentry:Release", "SENTRY_RELEASE");

                var tracesSampleRateStr = Get("Sentry:TracesSampleRate", "SENTRY_TRACES_SAMPLE_RATE");
                options.TracesSampleRate = double.TryParse(tracesSampleRateStr, out var rate) ? rate : 0.05;

                options.SendDefaultPii = false; // segurança por padrão
                options.IsGlobalModeEnabled = true; // captura fora do contexto de request
            });
        }
    }
}