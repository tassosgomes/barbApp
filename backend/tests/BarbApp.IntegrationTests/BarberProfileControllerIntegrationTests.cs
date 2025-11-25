using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.IntegrationTests;

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
        _factory = dbFixture.CreateFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.ResetDatabaseAsync();
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
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document = Document.Create("12345678000195");
        var code = UniqueCode.Create("ABC23456");
        var barbearia = Barbershop.Create(
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
        var barber = Barber.Create(
            _testBarbeariaId,
            "Carlos Santos",
            "carlos@teste.com",
            "Password123!",
            "11987654321"
        );

        dbContext.Barbers.Add(barber);
        await dbContext.SaveChangesAsync();

        _testBarberId = barber.Id;

        // Set JWT token for Barber
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _testBarberId.ToString(),
            userType: "Barbeiro",
            email: "carlos@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetProfile_ValidBarberToken_ShouldReturn200AndProfile()
    {
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
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task GetProfile_NoAuthentication_ShouldReturn401()
    {
        // Arrange - Create client without authentication
        var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WrongRole_ShouldReturn403()
    {
        // Arrange - Create client with Cliente role instead of Barbeiro
        var clientToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "Cliente",
            email: "cliente@teste.com",
            barbeariaId: _testBarbeariaId
        );

        var clientWithWrongRole = _factory.CreateClient();
        clientWithWrongRole.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientToken);

        // Act
        var response = await clientWithWrongRole.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProfile_InvalidToken_ShouldReturn401()
    {
        // Arrange - Create client with invalid token (malformed NameIdentifier)
        var invalidToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: "invalid-guid", // Invalid GUID format
            userType: "Barbeiro",
            email: "invalid@teste.com",
            barbeariaId: _testBarbeariaId
        );

        var clientWithInvalidToken = _factory.CreateClient();
        clientWithInvalidToken.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

        // Act
        var response = await clientWithInvalidToken.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_BarberNotFound_ShouldReturn404()
    {
        // Arrange - Create client with valid token but non-existent barber ID
        var nonExistentBarberId = Guid.NewGuid();
        var tokenForNonExistentBarber = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: nonExistentBarberId.ToString(),
            userType: "Barbeiro",
            email: "nonexistent@teste.com",
            barbeariaId: _testBarbeariaId
        );

        var clientWithNonExistentBarber = _factory.CreateClient();
        clientWithNonExistentBarber.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenForNonExistentBarber);

        // Act
        var response = await clientWithNonExistentBarber.GetAsync("/api/barber/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProfile_InactiveBarber_ShouldReturnProfile()
    {
        // Arrange - Deactivate the barber
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var barber = await dbContext.Barbers.FindAsync(_testBarberId);
        barber.Should().NotBeNull();
        barber!.Deactivate();
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/barber/profile");

        // Assert - Should still return profile, just with IsActive = false
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BarberProfileOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testBarberId);
        result.IsActive.Should().BeFalse();
    }
}