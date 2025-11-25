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
public class MultiTenantIsolationIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _barbearia1Id;
    private Guid _barbearia2Id;
    private string _barbearia1Code;
    private string _barbearia2Code;
    private string _cliente1Token;
    private string _cliente2Token;

    public MultiTenantIsolationIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = dbFixture.CreateFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.ResetDatabaseAsync();
        _factory.EnsureDatabaseInitialized();
        await SetupTestData();
        await SetupClientsAndTokens();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SetupTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        // Create two barbearias
        var address1 = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document1 = Document.Create("11111111000191");
        var code1 = UniqueCode.Create("TNANT234");
        var barbearia1 = Barbershop.Create(
            "Barbearia Tenant 1",
            document1,
            "(11) 98765-4321",
            "João Silva",
            "joao@tenant1.com",
            address1,
            code1,
            "test"
        );

        var address2 = Address.Create("01310101", "Av. Brigadeiro", "1001", null, "Jardim Paulista", "São Paulo", "SP");
        var document2 = Document.Create("22222222000192");
        var code2 = UniqueCode.Create("TNANT245");
        var barbearia2 = Barbershop.Create(
            "Barbearia Tenant 2",
            document2,
            "(11) 98765-4322",
            "Maria Silva",
            "maria@tenant2.com",
            address2,
            code2,
            "test"
        );

        dbContext.Addresses.Add(address1);
        dbContext.Addresses.Add(address2);
        dbContext.Barbershops.Add(barbearia1);
        dbContext.Barbershops.Add(barbearia2);
        await dbContext.SaveChangesAsync();

        _barbearia1Id = barbearia1.Id;
        _barbearia2Id = barbearia2.Id;
        _barbearia1Code = code1.Value;
        _barbearia2Code = code2.Value;

        // Create barbers for each barbearia
        var barber1 = Barber.Create(_barbearia1Id, "Barbeiro 1", "barbeiro1@tenant1.com", "Password123!", "(11) 99999-1111", new List<Guid>());
        var barber2 = Barber.Create(_barbearia2Id, "Barbeiro 2", "barbeiro2@tenant2.com", "Password123!", "(11) 99999-2222", new List<Guid>());

        dbContext.Barbers.Add(barber1);
        dbContext.Barbers.Add(barber2);

        // Create services for each barbearia
        var service1 = BarbershopService.Create(_barbearia1Id, "Corte Masculino", "Corte de cabelo masculino", 30, 25.00m);
        var service2 = BarbershopService.Create(_barbearia2Id, "Corte Feminino", "Corte de cabelo feminino", 45, 35.00m);

        dbContext.BarbershopServices.Add(service1);
        dbContext.BarbershopServices.Add(service2);

        await dbContext.SaveChangesAsync();
    }

    private async Task SetupClientsAndTokens()
    {
        // Create clients and get tokens
        var loginInput1 = new LoginClienteInput
        {
            CodigoBarbearia = _barbearia1Code,
            Telefone = "11911111111",
            Nome = "Cliente Tenant 1"
        };

        var loginInput2 = new LoginClienteInput
        {
            CodigoBarbearia = _barbearia2Code,
            Telefone = "11922222222",
            Nome = "Cliente Tenant 2"
        };

        var response1 = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput1);
        var response2 = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput2);

        var authResponse1 = await response1.Content.ReadFromJsonAsync<AuthResponse>();
        var authResponse2 = await response2.Content.ReadFromJsonAsync<AuthResponse>();

        _cliente1Token = authResponse1!.Token;
        _cliente2Token = authResponse2!.Token;
    }

    [Fact]
    public async Task GetBarbeiros_ClienteTenant1_NaoDeveVerBarbeirosTenant2()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cliente1Token);

        // Act
        var response = await _client.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var barbeiros = await response.Content.ReadFromJsonAsync<List<BarbeiroDto>>();
        barbeiros.Should().NotBeNull();
        barbeiros!.Should().HaveCount(1);
        barbeiros[0].Nome.Should().Be("Barbeiro 1");
    }

    [Fact]
    public async Task GetBarbeiros_ClienteTenant2_NaoDeveVerBarbeirosTenant1()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cliente2Token);

        // Act
        var response = await _client.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var barbeiros = await response.Content.ReadFromJsonAsync<List<BarbeiroDto>>();
        barbeiros.Should().NotBeNull();
        barbeiros!.Should().HaveCount(1);
        barbeiros[0].Nome.Should().Be("Barbeiro 2");
    }

    [Fact]
    public async Task GetServicos_ClienteTenant1_NaoDeveVerServicosTenant2()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cliente1Token);

        // Act
        var response = await _client.GetAsync("/api/servicos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var servicos = await response.Content.ReadFromJsonAsync<List<ServicoDto>>();
        servicos.Should().NotBeNull();
        servicos!.Should().HaveCount(1);
        servicos[0].Nome.Should().Be("Corte Masculino");
    }

    [Fact]
    public async Task GetServicos_ClienteTenant2_NaoDeveVerServicosTenant1()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cliente2Token);

        // Act
        var response = await _client.GetAsync("/api/servicos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var servicos = await response.Content.ReadFromJsonAsync<List<ServicoDto>>();
        servicos.Should().NotBeNull();
        servicos!.Should().HaveCount(1);
        servicos[0].Nome.Should().Be("Corte Feminino");
    }

    [Fact]
    public async Task PostAgendamento_ClienteTenant1_NaoDeveAgendarComBarbeiroTenant2()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cliente1Token);

        // Get barber from tenant 2 (should not be accessible)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var barberTenant2 = await dbContext.Barbers
            .FirstAsync(b => b.BarbeariaId == _barbearia2Id);
        var serviceTenant2 = await dbContext.BarbershopServices
            .FirstAsync(s => s.BarbeariaId == _barbearia2Id);

        var agendamentoInput = new CriarAgendamentoInput(
            BarbeiroId: barberTenant2.Id,
            ServicosIds: new List<Guid> { serviceTenant2.Id },
            DataHora: DateTime.Today.AddDays(1).AddHours(10) // 10:00 AM - within allowed range
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/agendamentos", agendamentoInput);

        // Assert - Should fail because barber is not accessible to this tenant
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAgendamentosMeus_ClienteTenant1_NaoDeveVerAgendamentosTenant2()
    {
        // Arrange - Create appointments for both tenants
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var cliente1 = await dbContext.Customers.FirstAsync(c => c.BarbeariaId == _barbearia1Id);
        var cliente2 = await dbContext.Customers.FirstAsync(c => c.BarbeariaId == _barbearia2Id);
        var barber1 = await dbContext.Barbers.FirstAsync(b => b.BarbeariaId == _barbearia1Id);
        var barber2 = await dbContext.Barbers.FirstAsync(b => b.BarbeariaId == _barbearia2Id);
        var service1 = await dbContext.BarbershopServices.FirstAsync(s => s.BarbeariaId == _barbearia1Id);
        var service2 = await dbContext.BarbershopServices.FirstAsync(s => s.BarbeariaId == _barbearia2Id);

        var agendamento1 = Agendamento.Create(
            _barbearia1Id, cliente1.Id, barber1.Id, new List<Guid> { service1.Id },
            DateTime.UtcNow.AddDays(1), service1.DurationMinutes);

        var agendamento2 = Agendamento.Create(
            _barbearia2Id, cliente2.Id, barber2.Id, new List<Guid> { service2.Id },
            DateTime.UtcNow.AddDays(2), service2.DurationMinutes);

        dbContext.Agendamentos.Add(agendamento1);
        dbContext.Agendamentos.Add(agendamento2);
        await dbContext.SaveChangesAsync();

        // Act - Client 1 gets their appointments
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cliente1Token);
        var response = await _client.GetAsync("/api/agendamentos/meus");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var agendamentos = await response.Content.ReadFromJsonAsync<List<AgendamentoOutput>>();
        agendamentos.Should().NotBeNull();
        agendamentos!.Should().HaveCount(1);
        agendamentos[0].Barbeiro.Id.Should().Be(barber1.Id);
    }

    [Fact]
    public async Task DeleteAgendamento_ClienteTenant1_NaoDeveDeletarAgendamentoTenant2()
    {
        // Arrange - Create appointments for both tenants
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var cliente1 = await dbContext.Customers.FirstAsync(c => c.BarbeariaId == _barbearia1Id);
        var cliente2 = await dbContext.Customers.FirstAsync(c => c.BarbeariaId == _barbearia2Id);
        var barber1 = await dbContext.Barbers.FirstAsync(b => b.BarbeariaId == _barbearia1Id);
        var barber2 = await dbContext.Barbers.FirstAsync(b => b.BarbeariaId == _barbearia2Id);
        var service1 = await dbContext.BarbershopServices.FirstAsync(s => s.BarbeariaId == _barbearia1Id);
        var service2 = await dbContext.BarbershopServices.FirstAsync(s => s.BarbeariaId == _barbearia2Id);

        var agendamento1 = Agendamento.Create(
            _barbearia1Id, cliente1.Id, barber1.Id, new List<Guid> { service1.Id },
            DateTime.UtcNow.AddDays(1), service1.DurationMinutes);

        var agendamento2 = Agendamento.Create(
            _barbearia2Id, cliente2.Id, barber2.Id, new List<Guid> { service2.Id },
            DateTime.UtcNow.AddDays(2), service2.DurationMinutes);

        dbContext.Agendamentos.Add(agendamento1);
        dbContext.Agendamentos.Add(agendamento2);
        await dbContext.SaveChangesAsync();

        // Act - Client 1 tries to delete Client 2's appointment
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cliente1Token);
        var response = await _client.DeleteAsync($"/api/agendamentos/{agendamento2.Id}");

        // Assert - Should not find the appointment (404) or get forbidden (403)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SemToken_DeveRetornar401()
    {
        // Arrange - No authorization header

        // Act
        var response = await _client.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenInvalido_DeveRetornar401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}