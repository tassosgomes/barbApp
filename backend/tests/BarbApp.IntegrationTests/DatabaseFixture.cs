using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

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

        //using var context = new BarbAppDbContext(optionsBuilder.Options);
        //context.Database.Migrate();
    }
}
