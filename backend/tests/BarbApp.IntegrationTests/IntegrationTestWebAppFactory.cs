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
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace BarbApp.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer;
    private bool _dbInitialized;
    private readonly object _dbLock = new();
    private bool _containerStarted;

    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("barbapp_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        // Start container in constructor
        _dbContainer.StartAsync().GetAwaiter().GetResult();
        _containerStarted = true;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Configure JWT settings for testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "test-secret-key-at-least-32-characters-long-for-jwt",
                ["JwtSettings:Issuer"] = "BarbApp-Test",
                ["JwtSettings:Audience"] = "BarbApp-Test-Users",
                ["JwtSettings:ExpirationMinutes"] = "60"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BarbAppDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove any existing DbContext registration by implementation type
            var dbContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(BarbAppDbContext));
            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            // Add test DbContext with PostgreSQL
            services.AddDbContext<BarbAppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
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
    /// Runs database migrations. Call this once after the first CreateClient() call.
    /// Thread-safe and idempotent - safe to call multiple times.
    /// </summary>
    public void EnsureDatabaseInitialized()
    {
        if (_dbInitialized)
            return;

        lock (_dbLock)
        {
            if (_dbInitialized)
                return;

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
            dbContext.Database.Migrate();
            _dbInitialized = true;
        }
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
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
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

    protected override void Dispose(bool disposing)
    {
        if (disposing && _containerStarted)
        {
            _dbContainer.StopAsync().GetAwaiter().GetResult();
            _dbContainer.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _containerStarted = false;
        }
        base.Dispose(disposing);
    }
}