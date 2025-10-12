using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BarbApp.IntegrationTests.Auth;

public class AuthenticationIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public AuthenticationIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
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

        var (barbeariaId, barbeiroId, telefone, codigo) =
            await TestHelper.CreateBarbeiroAsync(context);

        var loginInput = new LoginBarbeiroInput
        {
            Codigo = codigo,
            Telefone = telefone
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
            Codigo = "INVALID",
            Telefone = "11999999999"
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
        var (barbearia1Id, barbeiro1Id, telefone1, codigo1) =
            await TestHelper.CreateBarbeiroAsync(context);

        // Barbearia 2 with barber
        var (barbearia2Id, barbeiro2Id, telefone2, codigo2) =
            await TestHelper.CreateBarbeiroAsync(context);

        // Authenticate as barber from barbearia1
        var loginInput = new LoginBarbeiroInput
        {
            Codigo = codigo1,
            Telefone = telefone1
        };

        var authResponse = await _client.PostAsJsonAsync("/api/auth/barbeiro/login", loginInput);
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResult = await authResponse.Content.ReadFromJsonAsync<AuthResponse>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();

        // Debug: Print token for inspection
        Console.WriteLine($"JWT Token: {authResult.Token}");

        // Set the JWT token in the Authorization header for subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.Token);

        // Test authentication with weather forecast endpoint first
        var weatherResponse = await _client.GetAsync("/weatherforecast");
        Console.WriteLine($"Weather forecast status: {weatherResponse.StatusCode}");

        // Act - List barbers (should only see barbers from barbearia1)
        var listResponse = await _client.GetAsync("/api/auth/barbeiros");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var barbers = await listResponse.Content.ReadFromJsonAsync<List<BarberInfo>>();
        barbers.Should().NotBeNull();
        barbers!.Should().HaveCount(1);
        barbers[0].Id.Should().Be(barbeiro1Id);
        barbers[0].Telefone.Should().Be(telefone1);
        barbers[0].BarbeariaId.Should().Be(barbearia1Id);
    }
}