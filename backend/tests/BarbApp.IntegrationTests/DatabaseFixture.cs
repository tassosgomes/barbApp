using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using BarbApp.Infrastructure.Services;

namespace BarbApp.IntegrationTests;

/// <summary>
/// Manages the PostgreSQL container lifecycle for all integration tests.
/// This is shared across all tests in the collection.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public string ConnectionString { get; private set; } = string.Empty;

    public DatabaseFixture()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("barbapp_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        ConnectionString = _dbContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    /// <summary>
    /// Runs database migrations. Should be called once after the container starts.
    /// </summary>
    public void RunMigrations()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BarbAppDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        using var context = new BarbAppDbContext(optionsBuilder.Options, new TenantContext());
        
        // Ensure database exists
        context.Database.EnsureCreated();
        
        // Then run migrations to ensure schema is up to date
        try
        {
            context.Database.Migrate();
        }
        catch (Exception ex) when (ex.Message.Contains("already exists") || ex.InnerException?.Message.Contains("already exists") == true)
        {
            // Migrations already applied, continue
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
        {
            // Tables don't exist, try to recreate schema
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.Database.Migrate();
        }
    }
}
