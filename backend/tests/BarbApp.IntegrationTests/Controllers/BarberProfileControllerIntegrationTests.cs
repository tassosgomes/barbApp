using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace BarbApp.IntegrationTests.Controllers;

[Collection(nameof(IntegrationTestCollection))]
public class BarberProfileControllerIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _testBarbeariaId;
    private Guid _testBarberId;

    public BarberProfileControllerIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = new IntegrationTestWebAppFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Ensure database is initialized
        _factory.EnsureDatabaseInitialized();

        // Setup test data
        await SetupTestData();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SetupTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        // Create test barbearia
        var address = BarbApp.Domain.Entities.Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document = BarbApp.Domain.ValueObjects.Document.Create("12345678000195");
        var code = BarbApp.Domain.ValueObjects.UniqueCode.Create("BARBTEST");
        var barbearia = BarbApp.Domain.Entities.Barbershop.Create(
            "Barbearia Teste",
            document,
            "(11) 98765-4321",
            "João Silva",
            "joao@teste.com",
            address,
            code,
            "test"
        );

        dbContext.Addresses.Add(address);
        dbContext.Barbershops.Add(barbearia);
        await dbContext.SaveChangesAsync();

        _testBarbeariaId = barbearia.Id;

        // Create test barber
        var barber = BarbApp.Domain.Entities.Barber.Create(
            _testBarbeariaId,
            "Carlos Santos",
            "carlos@teste.com",
            "Password123!",
            "11987654321",
            new List<Guid>()
        );

        dbContext.Barbers.Add(barber);
        await dbContext.SaveChangesAsync();

        _testBarberId = barber.Id;
    }

    [Fact]
    public async Task GetProfile_ValidBarberToken_ShouldReturn200AndProfile()
    {
        // Arrange
        var barberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _testBarberId.ToString(),
            userType: "Barbeiro",
            email: "carlos@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", barberToken);

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BarberProfileOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testBarberId);
        result.Name.Should().Be("Carlos Santos");
        result.Email.Should().Be("carlos@teste.com");
        result.PhoneNumber.Should().Be("11987654321");
        result.IsActive.Should().BeTrue();
        result.BarbeariaId.Should().Be(_testBarbeariaId);
        result.BarbeariaNome.Should().Be("Barbearia Teste");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task GetProfile_NoAuthentication_ShouldReturn401()
    {
        // Arrange - No token

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_InvalidToken_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_AdminBarbeariaToken_ShouldReturn403()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProfile_AdminCentralToken_ShouldReturn403()
    {
        // Arrange
        var adminCentralToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminCentral",
            email: "admin.central@test.com"
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminCentralToken);

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProfile_InvalidBarberIdInToken_ShouldReturn404()
    {
        // Arrange
        var invalidBarberId = Guid.NewGuid();
        var barberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: invalidBarberId.ToString(),
            userType: "Barbeiro",
            email: "invalid@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", barberToken);

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        errorResponse.Should().ContainKey("message");
        errorResponse!["message"].ToString().Should().Be("Barbeiro não encontrado");
    }

    [Fact]
    public async Task GetProfile_MissingNameIdentifierClaim_ShouldReturn401()
    {
        // Arrange - Create token without NameIdentifier claim
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "carlos@teste.com"),
            new(ClaimTypes.Role, "Barbeiro"),
            new("userType", "Barbeiro"),
            new("barbeariaId", _testBarbeariaId.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key-at-least-32-characters-long-for-jwt"));
        securityKey.KeyId = "test-key";
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "BarbApp-Test",
            audience: "BarbApp-Test-Users",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenWithoutNameId = new JwtSecurityTokenHandler().WriteToken(token);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenWithoutNameId);

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        errorResponse.Should().ContainKey("message");
        errorResponse!["message"].ToString().Should().Be("Token inválido");
    }

    [Fact]
    public async Task GetProfile_InvalidGuidInNameIdentifierClaim_ShouldReturn401()
    {
        // Arrange - Token with invalid GUID in NameIdentifier claim
        var tokenWithInvalidGuid = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: "not-a-guid",
            userType: "Barbeiro",
            email: "carlos@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenWithInvalidGuid);

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        errorResponse.Should().ContainKey("message");
        errorResponse!["message"].ToString().Should().Be("Token inválido");
    }

    [Fact]
    public async Task GetProfile_InactiveBarber_ShouldReturn200WithInactiveStatus()
    {
        // Arrange - Deactivate the barber first
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var barber = await dbContext.Barbers.FindAsync(_testBarberId);
        barber!.Deactivate();
        await dbContext.SaveChangesAsync();

        var barberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _testBarberId.ToString(),
            userType: "Barbeiro",
            email: "carlos@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", barberToken);

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BarberProfileOutput>();
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }
}