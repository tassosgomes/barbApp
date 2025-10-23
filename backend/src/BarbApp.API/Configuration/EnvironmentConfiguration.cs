using Microsoft.Extensions.Configuration;
using DotNetEnv;

namespace BarbApp.API.Configuration;

public static class EnvironmentConfiguration
{
    public static void LoadEnvironmentVariables()
    {
        // Load environment variables from .env file
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
    }

    public static void ConfigureEnvironmentOverrides(WebApplicationBuilder builder)
    {
        // Override configuration with environment variables
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
    }

    public static void ConfigureTestingEnvironment(WebApplicationBuilder builder)
    {
        // Testing environment defaults
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
    }
}