using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BarbApp.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class BarbershopServicesControllerIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _testBarbeariaId;
    private Guid _otherBarbeariaId;
    private List<Guid> _testServiceIds;

    public BarbershopServicesControllerIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = dbFixture.CreateFactory();
        _client = _factory.CreateClient();
        _testServiceIds = new List<Guid>();
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

        // Create test barbearias
        var address1 = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document1 = Document.Create("12345678000195");
        var code1 = UniqueCode.Create("ABC23456");
        var barbearia1 = Barbershop.Create(
            "Barbearia Teste 1",
            document1,
            "(11) 98765-4321",
            "João Silva",
            "joao@teste.com",
            address1,
            code1,
            "test"
        );

        var address2 = Address.Create("01310101", "Av. Brigadeiro", "1001", null, "Jardim Paulista", "São Paulo", "SP");
        var document2 = Document.Create("98765432000189");
        var code2 = UniqueCode.Create("DEF56789");
        var barbearia2 = Barbershop.Create(
            "Barbearia Teste 2",
            document2,
            "(11) 98765-4322",
            "Maria Silva",
            "maria@teste.com",
            address2,
            code2,
            "test"
        );

        dbContext.Addresses.AddRange(address1, address2);
        dbContext.Barbershops.AddRange(barbearia1, barbearia2);
        await dbContext.SaveChangesAsync();

        _testBarbeariaId = barbearia1.Id;
        _otherBarbeariaId = barbearia2.Id;

        // Create test services for barbearia1
        var service1 = BarbershopService.Create(_testBarbeariaId, "Corte Masculino", "Corte completo", 30, 25.00m);
        var service2 = BarbershopService.Create(_testBarbeariaId, "Barba", "Aparar barba", 15, 15.00m);
        var service3 = BarbershopService.Create(_testBarbeariaId, "Sobrancelha", "Depilação sobrancelha", 10, 10.00m);

        dbContext.BarbershopServices.AddRange(service1, service2, service3);
        await dbContext.SaveChangesAsync();

        _testServiceIds = new List<Guid> { service1.Id, service2.Id, service3.Id };

        // Set JWT token for AdminBarbearia with barbeariaId
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@barbearia.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CreateService_ValidData_ShouldReturn201AndCreateService()
    {
        // Arrange
        var input = new CreateBarbershopServiceInput(
            Name: "Corte Feminino",
            Description: "Corte completo feminino",
            DurationMinutes: 45,
            Price: 35.00m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbershop-services", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<BarbershopServiceOutput>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Corte Feminino");
        result.Description.Should().Be("Corte completo feminino");
        result.DurationMinutes.Should().Be(45);
        result.Price.Should().Be(35.00m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ListServices_NoFilters_ShouldReturnServicesFromCurrentBarbearia()
    {
        // Act
        var listResponse = await _client.GetAsync("/api/barbershop-services?page=1&pageSize=10");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbershopServicesOutput>();
        result.Should().NotBeNull();
        result!.Services.Should().HaveCount(3);
        result.Services.Should().AllSatisfy(service =>
        {
            service.Id.Should().NotBeEmpty();
            service.Name.Should().NotBeNullOrEmpty();
            service.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task GetServiceById_ExistingId_ShouldReturn200AndService()
    {
        // Act
        var getResponse = await _client.GetAsync($"/api/barbershop-services/{_testServiceIds[0]}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<BarbershopServiceOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testServiceIds[0]);
        result.Name.Should().Be("Corte Masculino");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateService_ExistingId_ShouldReturn200AndUpdateService()
    {
        // Arrange
        var updateInput = new UpdateBarbershopServiceInput(
            Id: _testServiceIds[0],
            Name: "Corte Masculino Atualizado",
            Description: "Corte completo atualizado",
            DurationMinutes: 35,
            Price: 30.00m
        );

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/barbershop-services/{_testServiceIds[0]}", updateInput);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await updateResponse.Content.ReadFromJsonAsync<BarbershopServiceOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testServiceIds[0]);
        result.Name.Should().Be("Corte Masculino Atualizado");
        result.Description.Should().Be("Corte completo atualizado");
        result.DurationMinutes.Should().Be(35);
        result.Price.Should().Be(30.00m);
    }

    [Fact]
    public async Task DeleteService_ExistingId_ShouldReturn204AndDeactivate()
    {
        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/barbershop-services/{_testServiceIds[1]}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify service is deactivated (not returned in list by default)
        var listResponse = await _client.GetAsync("/api/barbershop-services?page=1&pageSize=100");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbershopServicesOutput>();
        listResult.Should().NotBeNull();
        listResult!.Services.Should().NotContain(service => service.Id == _testServiceIds[1]);
    }

    [Fact]
    public async Task AccessServices_WithoutAuth_ShouldReturn401()
    {
        // Arrange - Create client without auth
        var unauthClient = _factory.CreateClient();

        var input = new CreateBarbershopServiceInput(
            Name: "Serviço Sem Auth",
            Description: "Teste",
            DurationMinutes: 30,
            Price: 20.00m
        );

        // Act
        var response = await unauthClient.PostAsJsonAsync("/api/barbershop-services", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessServices_WithWrongRole_ShouldReturn403()
    {
        // Arrange - Use Cliente role instead of AdminBarbearia
        var clientToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "Cliente",
            email: "cliente@teste.com",
            barbeariaId: _testBarbeariaId
        );

        var clientWithWrongRole = _factory.CreateClient();
        clientWithWrongRole.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientToken);

        var input = new CreateBarbershopServiceInput(
            Name: "Acesso Negado",
            Description: "Teste",
            DurationMinutes: 30,
            Price: 20.00m
        );

        // Act
        var response = await clientWithWrongRole.PostAsJsonAsync("/api/barbershop-services", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AccessServices_FromOtherBarbearia_ShouldReturnEmptyList()
    {
        // Create client authenticated for other barbearia
        var tokenForOtherBarbearia = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@otherbarbearia.com",
            barbeariaId: _otherBarbeariaId
        );

        var clientForOtherBarbearia = _factory.CreateClient();
        clientForOtherBarbearia.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenForOtherBarbearia);

        // Act - Try to list services from other barbearia
        var listResponse = await clientForOtherBarbearia.GetAsync("/api/barbershop-services?page=1&pageSize=10");

        // Assert - Should return empty list (global query filter prevents access)
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbershopServicesOutput>();
        result.Should().NotBeNull();
        result!.Services.Should().BeEmpty();
    }
}