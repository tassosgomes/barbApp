using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarbApp.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AuthorizationIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _barbeariaId;
    private string _barbeariaCode = string.Empty;
    private Guid _barberId;
    private Guid _serviceId;
    private string _validToken = string.Empty;

    public AuthorizationIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = new IntegrationTestWebAppFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _factory.EnsureDatabaseInitialized();
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
        var document = Document.Create("11111111000191");
        var code = UniqueCode.Create("AUTHTEST");
        var barbearia = Barbershop.Create(
            "Barbearia Auth Test",
            document,
            "(11) 98765-4321",
            "João Silva",
            "joao@authtest.com",
            address,
            code,
            "test"
        );

        dbContext.Addresses.Add(address);
        dbContext.Barbershops.Add(barbearia);
        await dbContext.SaveChangesAsync();

        _barbeariaId = barbearia.Id;
        _barbeariaCode = code.Value;

        // Create barber
        var barber = Barber.Create(_barbeariaId, "Barbeiro Auth", "barbeiro@authtest.com", "Password123!", "(11) 99999-1111", new List<Guid>());
        dbContext.Barbers.Add(barber);
        await dbContext.SaveChangesAsync();
        _barberId = barber.Id;

        // Create service
        var service = BarbershopService.Create(_barbeariaId, "Corte Auth", "Corte para testes de autorização", 30, 25.00m);
        dbContext.BarbershopServices.Add(service);
        await dbContext.SaveChangesAsync();
        _serviceId = service.Id;

        // Login to get token
        var loginInput = new LoginClienteInput
        {
            CodigoBarbearia = _barbeariaCode,
            Telefone = "11999999999",
            Nome = "Cliente Auth Test"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _validToken = authResponse!.Token;
    }

    [Fact]
    public async Task PostAgendamento_TokenValido_DadosValidos_DeveRetornar201()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _validToken);

        var request = new CriarAgendamentoInput(
            BarbeiroId: _barberId,
            ServicosIds: new List<Guid> { _serviceId },
            DataHora: DateTime.Now.AddDays(1).Date.AddHours(10)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/agendamentos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetAgendamentos_TokenValido_DeveRetornar200()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _validToken);

        // Act
        var response = await _client.GetAsync("/api/agendamentos/meus");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostAgendamento_TokenInvalido_DeveRetornar401()
    {
        // Arrange
        var unauthClient = _factory.CreateClient();
        unauthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var request = new CriarAgendamentoInput(
            BarbeiroId: _barberId,
            ServicosIds: new List<Guid> { _serviceId },
            DataHora: DateTime.Now.AddDays(1).Date.AddHours(10)
        );

        // Act
        var response = await unauthClient.PostAsJsonAsync("/api/agendamentos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAgendamentos_TokenInvalido_DeveRetornar401()
    {
        // Arrange
        var unauthClient = _factory.CreateClient();
        unauthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await unauthClient.GetAsync("/api/agendamentos/meus");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessBarbeiros_TokenInvalido_DeveRetornar401()
    {
        // Arrange
        var unauthClient = _factory.CreateClient();
        unauthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await unauthClient.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessServicos_TokenInvalido_DeveRetornar401()
    {
        // Arrange
        var unauthClient = _factory.CreateClient();
        unauthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await unauthClient.GetAsync("/api/servicos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
