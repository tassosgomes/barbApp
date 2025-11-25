using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;
using BarbApp.Infrastructure.Services;

namespace BarbApp.IntegrationTests;

/// <summary>
/// Generates unique test data to avoid conflicts when tests run in parallel.
/// Each test class gets unique identifiers to prevent duplicate key violations.
/// </summary>
public static class TestDataGenerator
{
    private static long _counter = DateTime.UtcNow.Ticks;

    /// <summary>
    /// Generates a unique 8-character alphanumeric code for barbershops.
    /// </summary>
    public static string GenerateUniqueCode()
    {
        var id = Interlocked.Increment(ref _counter);
        // Generate a unique alphanumeric code using base36 (0-9, A-Z)
        return $"T{id % 10000000:D7}"[..8].ToUpperInvariant();
    }

    /// <summary>
    /// Generates a unique CNPJ-like document (14 digits).
    /// </summary>
    public static string GenerateUniqueCnpj()
    {
        var id = Interlocked.Increment(ref _counter);
        // Generate a unique 14-digit number (CNPJ format without check digits)
        return $"{id % 100000000:D8}0001{(id % 100):D2}";
    }

    /// <summary>
    /// Generates a unique email address.
    /// </summary>
    public static string GenerateUniqueEmail(string prefix = "test")
    {
        var id = Interlocked.Increment(ref _counter);
        return $"{prefix}.{id}@teste.com";
    }

    /// <summary>
    /// Generates a unique phone number.
    /// </summary>
    public static string GenerateUniquePhone()
    {
        var id = Interlocked.Increment(ref _counter);
        return $"(11) 9{(id % 10000):D4}-{(id % 10000):D4}";
    }

    /// <summary>
    /// Generates a unique CEP (postal code).
    /// </summary>
    public static string GenerateUniqueCep()
    {
        var id = Interlocked.Increment(ref _counter);
        return $"{(id % 100000):D5}{(id % 1000):D3}";
    }

    /// <summary>
    /// Generates a unique name with suffix.
    /// </summary>
    public static string GenerateUniqueName(string baseName)
    {
        var id = Interlocked.Increment(ref _counter);
        return $"{baseName} {id % 10000}";
    }
}

/// <summary>
/// Manages the PostgreSQL container lifecycle for all integration tests.
/// This fixture is shared across ALL test classes via ICollectionFixture.
/// Only ONE container is created for the entire test run.
/// 
/// Best Practice (from Testcontainers .NET docs):
/// Use ICollectionFixture to share a single container across multiple test classes,
/// reducing container startup overhead and improving test execution time.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private bool _databaseInitialized;
    private readonly object _initLock = new();
    private Respawner? _respawner;

    public string ConnectionString { get; private set; } = string.Empty;

    public DatabaseFixture()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("barbapp_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true) // Ensures container cleanup even on test failure
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        ConnectionString = _dbContainer.GetConnectionString();
        
        // Initialize database schema once when container starts
        EnsureDatabaseInitialized();

        // Initialize Respawner for database cleanup between tests
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            // Don't clean the __EFMigrationsHistory table if using migrations
            TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
        });
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    /// <summary>
    /// Resets the database to a clean state.
    /// Call this at the beginning of each test class's InitializeAsync() method.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner == null)
            return;

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Ensures the database is initialized with the correct schema.
    /// Thread-safe and idempotent - called once during fixture initialization.
    /// </summary>
    private void EnsureDatabaseInitialized()
    {
        if (_databaseInitialized)
            return;

        lock (_initLock)
        {
            if (_databaseInitialized)
                return;

            var optionsBuilder = new DbContextOptionsBuilder<BarbAppDbContext>();
            optionsBuilder.UseNpgsql(ConnectionString);

            using var context = new BarbAppDbContext(optionsBuilder.Options, new TenantContext());
            
            // Create database schema
            context.Database.EnsureCreated();
            
            _databaseInitialized = true;
        }
    }

    /// <summary>
    /// Creates a new IntegrationTestWebAppFactory using the shared connection string.
    /// Each test class should use this to create their factory instance.
    /// </summary>
    public IntegrationTestWebAppFactory CreateFactory()
    {
        return new IntegrationTestWebAppFactory(ConnectionString);
    }
}
