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
public class ClientControllersIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _testBarbeariaId;
    private Guid _otherBarbeariaId;
    private List<Guid> _testServiceIds;
    private Guid _testBarberId;

    public ClientControllersIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = new IntegrationTestWebAppFactory();
        _client = _factory.CreateClient();
        _testServiceIds = new List<Guid>();
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

        // Create test barbearias
        var address1 = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document1 = Document.Create("12345678000195");
        var code1 = UniqueCode.Create("BARBA223");
        var barbearia1 = Barbershop.Create(
            "Barbearia Teste 1",
            document1,
            "11987654321",
            "João Silva",
            "joao@teste.com",
            address1,
            code1,
            "test"
        );

        var address2 = Address.Create("01310101", "Av. Brigadeiro", "1001", null, "Jardim Paulista", "São Paulo", "SP");
        var document2 = Document.Create("98765432000189");
        var code2 = UniqueCode.Create("BARBB224");
        var barbearia2 = Barbershop.Create(
            "Barbearia Teste 2",
            document2,
            "11987654322",
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

        // Create test barber for barbearia1
        var barber = Barber.Create(
            _testBarbeariaId,
            "Carlos Santos",
            "carlos@teste.com",
            "Password123!",
            "11987654323",
            new List<Guid> { service1.Id, service2.Id }
        );

        dbContext.Barbers.Add(barber);
        await dbContext.SaveChangesAsync();

        _testBarberId = barber.Id;

        // Create test customer for barbearia1
        var cliente = Cliente.Create(_testBarbeariaId, "Cliente Teste", "11999999999");
        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync();

        // Set JWT token for Cliente with barbeariaId
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: cliente.Id.ToString(),
            userType: "Cliente",
            email: "cliente@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetBarbeiros_TokenBarbeariaA_DeveRetornarApenasBarbeirosDeBarbeariaA()
    {
        // Arrange: Criar 2 barbearias com barbeiros diferentes
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        // Create barber for other barbearia
        var otherBarber = Barber.Create(
            _otherBarbeariaId,
            "Pedro Oliveira",
            "pedro@teste.com",
            "Password123!",
            "11987654324",
            new List<Guid>()
        );

        dbContext.Barbers.Add(otherBarber);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var barbeiros = await response.Content.ReadFromJsonAsync<List<BarbeiroDto>>();
        barbeiros.Should().NotBeNull();
        barbeiros.Should().ContainSingle();
        barbeiros![0].Nome.Should().Be("Carlos Santos");
        barbeiros.Should().NotContain(b => b.Nome.Contains("Pedro"));
    }

    [Fact]
    public async Task GetServicos_DeveRetornarServicosDaBarbearia()
    {
        // Act
        var response = await _client.GetAsync("/api/servicos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var servicos = await response.Content.ReadFromJsonAsync<List<ServicoDto>>();
        servicos.Should().NotBeNull();
        servicos.Should().HaveCount(3);
        servicos!.Should().Contain(s => s.Nome == "Corte Masculino");
        servicos.Should().Contain(s => s.Nome == "Barba");
        servicos.Should().Contain(s => s.Nome == "Sobrancelha");
    }

    [Fact]
    public async Task PostAgendamentos_ClienteBarbeariaA_CriaAgendamentoValido_DeveRetornar201()
    {
        // Arrange
        var input = new CriarAgendamentoInput(
            BarbeiroId: _testBarberId,
            ServicosIds: new List<Guid> { _testServiceIds[0] }, // Corte Masculino
            DataHora: DateTime.UtcNow.AddDays(1).Date.AddHours(10) // Amanhã às 10h
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/agendamentos", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var agendamento = await response.Content.ReadFromJsonAsync<AgendamentoOutput>();
        agendamento.Should().NotBeNull();
        agendamento!.Barbeiro.Nome.Should().Be("Carlos Santos");
        agendamento.Servicos.Should().ContainSingle();
        agendamento.Servicos[0].Nome.Should().Be("Corte Masculino");
        agendamento.Status.Should().Be("Pendente");
    }

    [Fact]
    public async Task PostAgendamentos_ClienteBarbeariaA_TentaAgendarBarbeiroBarbeariaB_DeveRetornar403()
    {
        // Arrange: Criar barbeiro na barbearia B
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var barberBarbeariaB = Barber.Create(
            _otherBarbeariaId,
            "Roberto Lima",
            "roberto@teste.com",
            "Password123!",
            "11987654325",
            new List<Guid>()
        );

        dbContext.Barbers.Add(barberBarbeariaB);
        await dbContext.SaveChangesAsync();

        var input = new CriarAgendamentoInput(
            BarbeiroId: barberBarbeariaB.Id, // Barbeiro da barbearia B
            ServicosIds: new List<Guid> { _testServiceIds[0] },
            DataHora: DateTime.UtcNow.AddDays(1).Date.AddHours(10)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/agendamentos", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAgendamentosMeus_DeveRetornarApenasAgendamentosDoCliente()
    {
        // Arrange: Criar agendamento para o cliente
        var input = new CriarAgendamentoInput(
            BarbeiroId: _testBarberId,
            ServicosIds: new List<Guid> { _testServiceIds[0] },
            DataHora: DateTime.UtcNow.AddDays(2).Date.AddHours(14)
        );

        var createResponse = await _client.PostAsJsonAsync("/api/agendamentos", input);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await _client.GetAsync("/api/agendamentos/meus");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var agendamentos = await response.Content.ReadFromJsonAsync<List<AgendamentoOutput>>();
        agendamentos.Should().NotBeNull();
        agendamentos.Should().ContainSingle();
        agendamentos![0].Barbeiro.Nome.Should().Be("Carlos Santos");
    }

    [Fact]
    public async Task DeleteAgendamentos_ClienteNaoProprietario_DeveRetornar403()
    {
        // Arrange: Cliente A cria agendamento
        var input = new CriarAgendamentoInput(
            BarbeiroId: _testBarberId,
            ServicosIds: new List<Guid> { _testServiceIds[0] },
            DataHora: DateTime.UtcNow.AddDays(3).Date.AddHours(16)
        );

        var createResponse = await _client.PostAsJsonAsync("/api/agendamentos", input);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var agendamento = await createResponse.Content.ReadFromJsonAsync<AgendamentoOutput>();
        agendamento.Should().NotBeNull();

        // Cliente B tenta cancelar (criar novo cliente/token)
        var tokenClienteB = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "Cliente",
            email: "cliente2@teste.com",
            barbeariaId: _testBarbeariaId
        );

        var clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenClienteB);

        // Act
        var response = await clientB.DeleteAsync($"/api/agendamentos/{agendamento!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PutAgendamentos_EditarProprioAgendamento_DeveRetornar200()
    {
        // Arrange: Criar agendamento
        var input = new CriarAgendamentoInput(
            BarbeiroId: _testBarberId,
            ServicosIds: new List<Guid> { _testServiceIds[0] },
            DataHora: DateTime.UtcNow.AddDays(4).Date.AddHours(9)
        );

        var createResponse = await _client.PostAsJsonAsync("/api/agendamentos", input);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var agendamento = await createResponse.Content.ReadFromJsonAsync<AgendamentoOutput>();
        agendamento.Should().NotBeNull();

        var editInput = new EditarAgendamentoInput(
            AgendamentoId: agendamento!.Id,
            BarbeiroId: _testBarberId,
            ServicosIds: new List<Guid> { _testServiceIds[1] }, // Mudar para Barba
            DataHora: DateTime.UtcNow.AddDays(4).Date.AddHours(11) // Mudar horário
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/agendamentos/{agendamento.Id}", editInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedAgendamento = await response.Content.ReadFromJsonAsync<AgendamentoOutput>();
        updatedAgendamento.Should().NotBeNull();
        updatedAgendamento!.Servicos.Should().ContainSingle();
        updatedAgendamento.Servicos[0].Nome.Should().Be("Barba");
    }

    [Fact]
    public async Task GetBarbeirosDisponibilidade_DeveRetornarDisponibilidade()
    {
        // Arrange
        var dataInicio = DateTime.UtcNow.AddDays(1).Date;
        var dataFim = dataInicio.AddDays(7);
        var duracaoMinutos = 30;

        // Act
        var response = await _client.GetAsync(
            $"/api/barbeiros/{_testBarberId}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoMinutos={duracaoMinutos}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var disponibilidade = await response.Content.ReadFromJsonAsync<DisponibilidadeOutput>();
        disponibilidade.Should().NotBeNull();
        disponibilidade!.Barbeiro.Id.Should().Be(_testBarberId);
        disponibilidade.DiasDisponiveis.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBarbeirosDisponibilidade_BarbeiroDeOutraBarbearia_DeveRetornar403()
    {
        // Arrange: Criar barbeiro na barbearia B
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var barberBarbeariaB = Barber.Create(
            _otherBarbeariaId,
            "José Santos",
            "jose@teste.com",
            "Password123!",
            "11987654326",
            new List<Guid>()
        );

        dbContext.Barbers.Add(barberBarbeariaB);
        await dbContext.SaveChangesAsync();

        var dataInicio = DateTime.UtcNow.AddDays(1).Date;
        var dataFim = dataInicio.AddDays(7);

        // Act
        var response = await _client.GetAsync(
            $"/api/barbeiros/{barberBarbeariaB.Id}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoMinutos=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AccessClientEndpoints_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessClientEndpoints_WithWrongRole_ShouldReturn403()
    {
        // Arrange - Use AdminBarbearia role instead of Cliente
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@teste.com",
            barbeariaId: _testBarbeariaId
        );

        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await adminClient.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}