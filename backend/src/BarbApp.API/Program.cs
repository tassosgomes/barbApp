using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ══════════════════════════════════════════════════════════
// LOAD ENVIRONMENT VARIABLES FROM .env FILE
// ══════════════════════════════════════════════════════════
// Carregar .env do diretório backend (não do diretório API)
var backendRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..");
var envPath = Path.Combine(backendRoot, ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"✓ Loaded .env from: {envPath}");
}
else
{
    Console.WriteLine($"⚠ .env file not found at: {envPath}");
    Console.WriteLine("  Environment variables will be loaded from system/container environment");
}

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════
// OVERRIDE CONFIGURATION WITH ENVIRONMENT VARIABLES
// ══════════════════════════════════════════════════════════
// Replace ${VAR} placeholders with actual environment values
var overrides = new Dictionary<string, string?>
{
    ["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
    ["JwtSettings:Secret"] = Environment.GetEnvironmentVariable("JWT_SECRET"),
    ["JwtSettings:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    ["JwtSettings:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
    ["AppSettings:FrontendUrl"] = Environment.GetEnvironmentVariable("FRONTEND_URL"),
    ["SmtpSettings:Host"] = Environment.GetEnvironmentVariable("SMTP_HOST"),
    ["SmtpSettings:Username"] = Environment.GetEnvironmentVariable("SMTP_USERNAME"),
    ["SmtpSettings:Password"] = Environment.GetEnvironmentVariable("SMTP_PASSWORD"),
    ["SmtpSettings:FromEmail"] = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL"),
    ["SmtpSettings:FromName"] = Environment.GetEnvironmentVariable("SMTP_FROM_NAME"),
    ["Sentry:Dsn"] = Environment.GetEnvironmentVariable("SENTRY_DSN"),
    ["Sentry:Environment"] = Environment.GetEnvironmentVariable("SENTRY_ENVIRONMENT"),
    ["Sentry:Release"] = Environment.GetEnvironmentVariable("SENTRY_RELEASE"),
    ["R2Storage:Endpoint"] = Environment.GetEnvironmentVariable("R2_ENDPOINT"),
    ["R2Storage:BucketName"] = Environment.GetEnvironmentVariable("R2_BUCKET_NAME"),
    ["R2Storage:PublicUrl"] = Environment.GetEnvironmentVariable("R2_PUBLIC_URL"),
    ["R2Storage:AccessKeyId"] = Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID"),
    ["R2Storage:SecretAccessKey"] = Environment.GetEnvironmentVariable("R2_SECRET_ACCESS_KEY")
};

// Only add non-null overrides
var nonNullOverrides = overrides
    .Where(kvp => kvp.Value != null)
    .Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value))
    .ToList();
    
if (nonNullOverrides.Any())
{
    builder.Configuration.AddInMemoryCollection(nonNullOverrides);
    Console.WriteLine($"✓ Applied {nonNullOverrides.Count} environment variable overrides");
}

// ══════════════════════════════════════════════════════════
// TESTING ENVIRONMENT DEFAULTS
// ══════════════════════════════════════════════════════════
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["JwtSettings:Secret"] = "test-secret-key-at-least-32-characters-long-for-jwt",
        ["JwtSettings:Issuer"] = "BarbApp-Test",
        ["JwtSettings:Audience"] = "BarbApp-Test-Users",
        ["JwtSettings:ExpirationMinutes"] = "60",
        ["Sentry:Dsn"] = string.Empty
    }!);
}

// ══════════════════════════════════════════════════════════
// LOGGING CONFIGURATION
// ══════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/barbapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ══════════════════════════════════════════════════════════
// SENTRY CONFIGURATION
// ══════════════════════════════════════════════════════════
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

// ══════════════════════════════════════════════════════════
// CONFIGURE SERVICES
// ══════════════════════════════════════════════════════════
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureInfrastructureServices();
builder.Services.ConfigureUseCases();
builder.Services.ConfigureFrameworkServices(builder);

// ══════════════════════════════════════════════════════════
// BUILD APPLICATION
// ══════════════════════════════════════════════════════════
var app = builder.Build();

// ══════════════════════════════════════════════════════════
// CONFIGURE MIDDLEWARE PIPELINE
// ══════════════════════════════════════════════════════════
app = app.ConfigureMiddleware();

// ══════════════════════════════════════════════════════════
// DATABASE MIGRATION (Development - Relational DB only)
// ══════════════════════════════════════════════════════════
await DatabaseConfiguration.RunDatabaseMigrations(app);

// ══════════════════════════════════════════════════════════
// RUN APPLICATION
// ══════════════════════════════════════════════════════════
try
{
    Log.Information("Starting BarbApp API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
