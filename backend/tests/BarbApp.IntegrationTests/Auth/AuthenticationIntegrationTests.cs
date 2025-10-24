using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BarbApp.IntegrationTests.Auth;

[Collection(nameof(IntegrationTestCollection))]
public class AuthenticationIntegrationTests
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _dbFixture;
    private static bool _dbInitialized;
    private static readonly object _initLock = new();
    private readonly ILogger _logger;

    public AuthenticationIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthenticationIntegrationTests>();

        // Initialize database once
        if (!_dbInitialized)
        {
            lock (_initLock)
            {
                if (!_dbInitialized)
                {
                    _dbFixture.RunMigrations();
                    _dbInitialized = true;
                }
            }
        }

        // Create factory with inline configuration
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
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
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BarbAppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Add test DbContext
                    services.AddDbContext<BarbAppDbContext>(options =>
                        options.UseNpgsql(_dbFixture.ConnectionString));
                });

                builder.UseEnvironment("Testing");
            });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task LoginAdminCentral_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var (id, email, senha) = await TestHelper.CreateAdminCentralAsync(context, passwordHasher);

        var loginInput = new LoginAdminCentralInput
        {
            Email = email,
            Senha = senha
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-central/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("AdminCentral");
        authResponse.BarbeariaId.Should().BeNull();
    }

    [Fact]
    public async Task LoginAdminCentral_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var (id2, email2, senha2) = await TestHelper.CreateAdminCentralAsync(context, passwordHasher);

        var loginInput = new LoginAdminCentralInput
        {
            Email = email2,
            Senha = "WrongPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-central/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginAdminBarbearia_WithValidCredentials_ReturnsTokenWithBarbeariaId()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var (barbeariaId, adminId, email3, senha3, codigo) =
            await TestHelper.CreateAdminBarbeariaAsync(context, passwordHasher);

        var loginInput = new LoginAdminBarbeariaInput
        {
            Email = email3,
            Senha = senha3,
            Codigo = codigo
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-barbearia/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("AdminBarbearia");
        authResponse.BarbeariaId.Should().Be(barbeariaId);
    }

    [Fact]
    public async Task LoginBarbeiro_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var (barbeariaId, barbeiroId, email, senha) =
            await TestHelper.CreateBarbeiroAsync(context);

        var loginInput = new LoginBarbeiroInput
        {
            Email = email,
            Password = senha
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/barbeiro/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("Barbeiro");
        authResponse.BarbeariaId.Should().Be(barbeariaId);
    }

    [Fact]
    public async Task LoginBarbeiro_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginInput = new LoginBarbeiroInput
        {
            Email = "invalid@test.com",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/barbeiro/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "Test@123", "Email é obrigatório")]
    [InlineData("invalid-email", "Test@123", "Email inválido")]
    [InlineData("test@test.com", "", "Senha é obrigatória")]
    [InlineData("test@test.com", "123", "Senha deve ter no mínimo 6 caracteres")]
    public async Task LoginAdminCentral_WithInvalidInput_ReturnsBadRequest(
        string email, string senha, string expectedError)
    {
        // Arrange
        var loginInput = new LoginAdminCentralInput
        {
            Email = email,
            Senha = senha
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-central/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain(expectedError);
    }

    [Fact]
    public async Task LoginCliente_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var (barbeariaId, clienteId, telefone, nome, codigo) =
            await TestHelper.CreateClienteAsync(context);

        var loginInput = new LoginClienteInput
        {
            Codigo = codigo,
            Telefone = telefone,
            Nome = nome
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("Cliente");
        authResponse.BarbeariaId.Should().Be(barbeariaId);
    }

    [Fact]
    public async Task LoginCliente_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginInput = new LoginClienteInput
        {
            Codigo = "INVALID",
            Telefone = "11999999999",
            Nome = "Test Cliente"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListBarbeiros_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Act - Make request without authentication
        var response = await _client.GetAsync("/api/auth/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrocarContexto_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var input = new { NovaBarbeariaId = Guid.NewGuid() };

        // Act - Make request without authentication
        var response = await _client.PostAsJsonAsync("/api/auth/barbeiro/trocar-contexto", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MultiTenant_Isolation_BarberCanOnlySeeOwnBarbeariaData()
    {
        // Arrange - Create barbers in 2 different barbearias
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        // Barbearia 1 with barber
        var (barbearia1Id, barbeiro1Id, email1, senha1) =
            await TestHelper.CreateBarbeiroAsync(context);

        // Barbearia 2 with barber
        var (barbearia2Id, barbeiro2Id, email2, senha2) =
            await TestHelper.CreateBarbeiroAsync(context);

        // Authenticate as barber from barbearia1
        var loginInput = new LoginBarbeiroInput
        {
            Email = email1,
            Password = senha1
        };

        var authResponse = await _client.PostAsJsonAsync("/api/auth/barbeiro/login", loginInput);
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResult = await authResponse.Content.ReadFromJsonAsync<AuthResponse>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();

        // Debug: Print token for inspection
        _logger.LogInformation("JWT Token: {Token}", authResult.Token);

        // Set the JWT token in the Authorization header for subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.Token);

        // Test authentication with weather forecast endpoint first
        var weatherResponse = await _client.GetAsync("/weatherforecast");
        _logger.LogInformation("Weather forecast status: {StatusCode}", weatherResponse.StatusCode);

        // Act - List barbers (should only see barbers from barbearia1)
        var listResponse = await _client.GetAsync("/api/auth/barbeiros");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var barbers = await listResponse.Content.ReadFromJsonAsync<List<BarberInfo>>();
        barbers.Should().NotBeNull();
        barbers!.Should().HaveCount(1);
        barbers[0].Id.Should().Be(barbeiro1Id);
        barbers[0].BarbeariaId.Should().Be(barbearia1Id);
    }
}