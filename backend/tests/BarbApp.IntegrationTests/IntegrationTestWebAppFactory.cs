using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace BarbApp.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that uses a shared database connection string.
/// This factory does NOT create its own container - it receives the connection string
/// from the DatabaseFixture which is shared across all tests via ICollectionFixture.
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private bool _dbInitialized;

    /// <summary>
    /// Creates a new IntegrationTestWebAppFactory using the shared connection string from DatabaseFixture.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string from the shared DatabaseFixture container.</param>
    public IntegrationTestWebAppFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Configure connection string for testing (used by health checks)
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["JwtSettings:Secret"] = "test-secret-key-at-least-32-characters-long-for-jwt",
                ["JwtSettings:Issuer"] = "BarbApp-Test",
                ["JwtSettings:Audience"] = "BarbApp-Test-Users",
                ["JwtSettings:ExpirationMinutes"] = "60",
                // SMTP settings (not used but required by SmtpEmailService if it gets instantiated)
                ["SmtpSettings:Host"] = "localhost",
                ["SmtpSettings:Port"] = "587",
                ["SmtpSettings:EnableSsl"] = "true",
                ["SmtpSettings:Username"] = "test",
                ["SmtpSettings:Password"] = "test",
                ["SmtpSettings:FromEmail"] = "test@test.com",
                ["SmtpSettings:FromName"] = "Test"
            }!);
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace IWebHostEnvironment with test implementation
            var webHostEnvironmentDescriptors = services.Where(
                d => d.ServiceType == typeof(IWebHostEnvironment)).ToList();
            foreach (var descriptor in webHostEnvironmentDescriptors)
            {
                services.Remove(descriptor);
            }
            
            services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());

            // Remove existing DbContext registration
            var dbContextDescriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<BarbAppDbContext>)).ToList();
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove any existing DbContext registration by implementation type
            var dbContextServiceDescriptors = services.Where(
                d => d.ServiceType == typeof(BarbAppDbContext)).ToList();
            foreach (var descriptor in dbContextServiceDescriptors)
            {
                services.Remove(descriptor);
            }

            // Add test DbContext with PostgreSQL using the shared connection string
            services.AddDbContext<BarbAppDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            // Remove ALL IEmailService registrations and replace with FakeEmailService for testing
            var emailServiceDescriptors = services.Where(
                d => d.ServiceType == typeof(BarbApp.Application.Interfaces.IEmailService)).ToList();
            foreach (var descriptor in emailServiceDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddScoped<BarbApp.Application.Interfaces.IEmailService, BarbApp.Infrastructure.Services.FakeEmailService>();

            // Register LocalLogoUploadService for testing
            var logoUploadDescriptors = services.Where(
                d => d.ServiceType == typeof(BarbApp.Application.Interfaces.ILogoUploadService)).ToList();
            foreach (var descriptor in logoUploadDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddScoped<BarbApp.Application.Interfaces.ILogoUploadService, BarbApp.Infrastructure.Services.LocalLogoUploadService>();

            // Register ImageSharpProcessor for testing
            var imageProcessorDescriptors = services.Where(
                d => d.ServiceType == typeof(BarbApp.Application.Interfaces.IImageProcessor)).ToList();
            foreach (var descriptor in imageProcessorDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddScoped<BarbApp.Application.Interfaces.IImageProcessor, BarbApp.Infrastructure.Services.ImageSharpProcessor>();

            // Replace ISecretManager with test implementation to avoid Infisical dependency
            var secretManagerDescriptors = services.Where(
                d => d.ServiceType == typeof(BarbApp.Infrastructure.Services.ISecretManager)).ToList();
            foreach (var descriptor in secretManagerDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddSingleton<BarbApp.Infrastructure.Services.ISecretManager, TestSecretManager>();

            // Replace IR2StorageService with test implementation to avoid R2 dependency
            var r2StorageDescriptors = services.Where(
                d => d.ServiceType == typeof(BarbApp.Infrastructure.Services.IR2StorageService)).ToList();
            foreach (var descriptor in r2StorageDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddSingleton<BarbApp.Infrastructure.Services.IR2StorageService, TestR2StorageService>();
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Gets the database context from a new scope. Use this to setup test data or run migrations.
    /// IMPORTANT: Must be called AFTER CreateClient() has been called at least once.
    /// </summary>
    public BarbAppDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
    }

    /// <summary>
    /// Ensures the database schema exists. The database is initialized once by DatabaseFixture,
    /// but each test class may call this to ensure the application context is ready.
    /// This method is idempotent - safe to call multiple times.
    /// </summary>
    public void EnsureDatabaseInitialized()
    {
        if (_dbInitialized)
            return;

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        
        // Ensure schema exists (this is idempotent - EnsureCreated does nothing if tables exist)
        dbContext.Database.EnsureCreated();
        
        _dbInitialized = true;
    }

    /// <summary>
    /// Generates a test JWT token without relying on DI.
    /// This method can be called from test constructors safely.
    /// </summary>
    public static string GenerateTestJwtToken(
        string userId,
        string userType,
        string email = "test@example.com",
        Guid? barbeariaId = null)
    {
        const string secret = "test-secret-key-at-least-32-characters-long-for-jwt";
        const string issuer = "BarbApp-Test";
        const string audience = "BarbApp-Test-Users";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    securityKey.KeyId = "test-key"; // match API signing key identifier
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, userType),
            new("userType", userType)
        };

        if (barbeariaId.HasValue)
        {
            claims.Add(new Claim("barbeariaId", barbeariaId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Configures a WebApplicationFactory to use NoOpEmailService for testing.
    /// Use this helper to configure any WebApplicationFactory<Program> instance.
    /// </summary>
    public static void ConfigureNoOpEmailService(IServiceCollection services)
    {
        // Remove all existing IEmailService registrations
        var emailServiceDescriptors = services
            .Where(d => d.ServiceType == typeof(BarbApp.Application.Interfaces.IEmailService))
            .ToList();
        
        foreach (var descriptor in emailServiceDescriptors)
        {
            services.Remove(descriptor);
        }
        
        // Add NoOpEmailService
        services.AddScoped<BarbApp.Application.Interfaces.IEmailService, NoOpEmailService>();
    }

    // Note: Container lifecycle is managed by DatabaseFixture, not by this factory
}

class TestWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "BarbApp.IntegrationTests";
    public IFileProvider ContentRootFileProvider { get; set; }
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; } = "Testing";
    public IFileProvider WebRootFileProvider { get; set; }
    public string WebRootPath { get; set; }

    public TestWebHostEnvironment()
    {
        ContentRootPath = Directory.GetCurrentDirectory();
        ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
        
        WebRootPath = Path.Combine(ContentRootPath, "wwwroot");
        Directory.CreateDirectory(WebRootPath);
        WebRootFileProvider = new PhysicalFileProvider(WebRootPath);
    }
}

/// <summary>
/// Test implementation of ISecretManager that returns test values without requiring Infisical.
/// </summary>
class TestSecretManager : BarbApp.Infrastructure.Services.ISecretManager
{
    public Task<string> GetSecretAsync(string secretName)
    {
        // Return test values for known secrets
        return secretName switch
        {
            "JWT_SECRET" => Task.FromResult("test-secret-key-at-least-32-characters-long-for-jwt"),
            _ => Task.FromResult($"test-value-for-{secretName}")
        };
    }
}

/// <summary>
/// Test implementation of IR2StorageService that doesn't require real R2 credentials.
/// </summary>
class TestR2StorageService : BarbApp.Infrastructure.Services.IR2StorageService
{
    public Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        // Return a fake URL for testing
        return Task.FromResult($"https://test-r2-storage.example.com/{fileName}");
    }

    public Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        // No-op for testing - always return success
        return Task.FromResult(true);
    }

    public Task<Stream> DownloadFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        // Return empty stream for testing
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public string GetPublicUrl(string fileKey)
    {
        // Return a fake public URL for testing
        return $"https://test-r2-storage.example.com/{fileKey}";
    }
}