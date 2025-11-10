using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarbApp.IntegrationTests.Controllers;

[Collection(nameof(IntegrationTestCollection))]
public class BarbersControllerIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _testBarbeariaId;
    private Guid _testBarberId;
    private Guid _testServiceId;

    public BarbersControllerIntegrationTests(DatabaseFixture dbFixture)
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

        // Create test service
        var service = BarbApp.Domain.Entities.BarbershopService.Create(
            _testBarbeariaId,
            "Corte de Cabelo",
            "Corte masculino completo",
            30,
            25.00m
        );

        dbContext.BarbershopServices.Add(service);
        await dbContext.SaveChangesAsync();

        _testServiceId = service.Id;
    }

    [Fact]
    public async Task CreateBarber_ValidInput_AdminBarbeariaToken_ShouldReturn201AndBarber()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<BarberOutput>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("João Silva");
        result.Email.Should().Be("joao.silva@test.com");
        result.PhoneFormatted.Should().Be("(11) 98765-4321");
        result.IsActive.Should().BeTrue();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBarber_DuplicateEmail_ShouldReturn409()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Create first barber
        await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Try to create duplicate
        var duplicateInput = new CreateBarberInput(
            Name: "João Silva 2",
            Email: "joao.silva@test.com", // Same email
            Password: "Password123!",
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", duplicateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateBarber_NoAuthentication_ShouldReturn401()
    {
        // Arrange
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateBarber_InvalidRole_ShouldReturn403()
    {
        // Arrange
        var barberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _testBarberId.ToString(),
            userType: "Barbeiro",
            email: "carlos@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", barberToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListBarbers_AdminBarbeariaToken_ShouldReturn200AndBarbers()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Act
        var response = await _client.GetAsync("/api/barbers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.Barbers.Should().NotBeEmpty();
        result.Barbers.Should().Contain(b => b.Name == "João Silva");
    }

    [Fact]
    public async Task ListBarbers_WithSearchName_ShouldReturnFilteredResults()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create barbers
        var barber1 = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var barber2 = new CreateBarberInput(
            Name: "Maria Santos",
            Email: "maria@test.com",
            Password: "Password123!",
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        await _client.PostAsJsonAsync("/api/barbers", barber1);
        await _client.PostAsJsonAsync("/api/barbers", barber2);

        // Act
        var response = await _client.GetAsync("/api/barbers?searchName=João");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.Barbers.Should().HaveCount(1);
        result.Barbers.First().Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task GetBarberById_ValidId_AdminBarbeariaToken_ShouldReturn200AndBarber()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        // Act
        var response = await _client.GetAsync($"/api/barbers/{createdBarber!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BarberOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBarber.Id);
        result.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task GetBarberById_InvalidId_ShouldReturn404()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/barbers/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBarber_ValidInput_AdminBarbeariaToken_ShouldReturn200AndUpdatedBarber()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        var updateInput = new UpdateBarberInput(
            Id: createdBarber!.Id,
            Name: "João Silva Atualizado",
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{createdBarber.Id}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BarberOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBarber.Id);
        result.Name.Should().Be("João Silva Atualizado");
        result.PhoneFormatted.Should().Be("(11) 98765-4322");
        result.Email.Should().Be("joao.silva@test.com"); // Email unchanged
    }

    [Fact]
    public async Task DeactivateBarber_ValidId_AdminBarbeariaToken_ShouldReturn204()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        // Act
        var response = await _client.PutAsync($"/api/barbers/{createdBarber!.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify barber is deactivated
        var getResponse = await _client.GetAsync($"/api/barbers/{createdBarber.Id}");
        var barber = await getResponse.Content.ReadFromJsonAsync<BarberOutput>();
        barber!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveBarber_ValidId_AdminBarbeariaToken_ShouldReturn204()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        // Act
        var response = await _client.DeleteAsync($"/api/barbers/{createdBarber!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify barber is deactivated (should still be retrievable but inactive)
        var getResponse = await _client.GetAsync($"/api/barbers/{createdBarber.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedBarber = await getResponse.Content.ReadFromJsonAsync<BarberOutput>();
        retrievedBarber.Should().NotBeNull();
        retrievedBarber!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ResetBarberPassword_ValidId_AdminBarbeariaToken_ShouldReturn200()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        // Act
        var response = await _client.PostAsync($"/api/barbers/{createdBarber!.Id}/reset-password", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Senha redefinida com sucesso");
    }

    [Fact]
    public async Task GetTeamSchedule_AdminBarbeariaToken_ShouldReturn200AndSchedule()
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
        var response = await _client.GetAsync("/api/barbers/schedule");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TeamScheduleOutput>();
        result.Should().NotBeNull();
        result!.Appointments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeamSchedule_WithSpecificBarberId_ShouldReturnFilteredSchedule()
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
        var response = await _client.GetAsync($"/api/barbers/schedule?barberId={_testBarberId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TeamScheduleOutput>();
        result.Should().NotBeNull();
        result!.Appointments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeamSchedule_WithSpecificDate_ShouldReturnScheduleForDate()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var specificDate = DateTime.UtcNow.Date.AddDays(-1); // Yesterday instead of tomorrow

        // Act
        var response = await _client.GetAsync($"/api/barbers/schedule?date={specificDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TeamScheduleOutput>();
        result.Should().NotBeNull();
        result!.Appointments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeamSchedule_NoAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/barbers/schedule");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTeamSchedule_InvalidRole_ShouldReturn403()
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
        var response = await _client.GetAsync("/api/barbers/schedule");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDisponibilidade_ValidBarberId_AdminBarbeariaToken_ShouldReturn200AndAvailability()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/barbers/{_testBarberId}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoServicosMinutos=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DisponibilidadeOutput>();
        result.Should().NotBeNull();
        result!.DiasDisponiveis.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDisponibilidade_BarberNotFound_ShouldReturn404()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidBarberId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/barbers/{invalidBarberId}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoServicosMinutos=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDisponibilidade_InvalidDateRange_ShouldReturn200AndProcessDates()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var dataInicio = DateTime.UtcNow.Date.AddDays(7);
        var dataFim = dataInicio.AddDays(-1); // End date before start date

        // Act
        var response = await _client.GetAsync($"/api/barbers/{_testBarberId}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoServicosMinutos=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // API doesn't validate date ranges
        var result = await response.Content.ReadFromJsonAsync<DisponibilidadeOutput>();
        result.Should().NotBeNull();
        result!.DiasDisponiveis.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDisponibilidade_InvalidDuration_ShouldReturn200AndProcessWithDuration()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/barbers/{_testBarberId}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoServicosMinutos=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // API doesn't validate duration
        var result = await response.Content.ReadFromJsonAsync<DisponibilidadeOutput>();
        result.Should().NotBeNull();
        result!.DiasDisponiveis.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDisponibilidade_NoAuthentication_ShouldReturn401()
    {
        // Arrange
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/barbers/{_testBarberId}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoServicosMinutos=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDisponibilidade_InvalidRole_ShouldReturn403()
    {
        // Arrange
        var barberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _testBarberId.ToString(),
            userType: "Barbeiro",
            email: "carlos@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", barberToken);

        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/barbers/{_testBarberId}/disponibilidade?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}&duracaoServicosMinutos=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateBarber_EmptyName_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "", // Empty name
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBarber_NameTooLong_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: new string('A', 101), // 101 characters, exceeds max of 100
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBarber_InvalidEmail_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "invalid-email", // Invalid email format
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBarber_EmailTooLong_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: new string('a', 247) + "@test.com", // 256 characters total, exceeds max of 255
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBarber_EmptyPassword_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "", // Empty password
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBarber_PasswordTooShort_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "1234567", // 7 characters, below minimum of 8
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBarber_InvalidPhone_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "123", // Invalid phone format
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBarber_NullServiceIds_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: null! // Null service IDs
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", createInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListBarbers_IsActiveFalse_ShouldReturnOnlyInactiveBarbers()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create active barber
        var activeBarber = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        await _client.PostAsJsonAsync("/api/barbers", activeBarber);

        // Create inactive barber
        var inactiveBarber = new CreateBarberInput(
            Name: "Maria Santos",
            Email: "maria@test.com",
            Password: "Password123!",
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", inactiveBarber);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();
        await _client.PutAsync($"/api/barbers/{createdBarber!.Id}/deactivate", null);

        // Act
        var response = await _client.GetAsync("/api/barbers?isActive=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.Barbers.Should().ContainSingle();
        result.Barbers.First().Name.Should().Be("Maria Santos");
        result.Barbers.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ListBarbers_InvalidPage_ShouldReturn200AndCorrectToPage1()
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
        var response = await _client.GetAsync("/api/barbers?page=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(1); // Should be corrected to 1
    }

    [Fact]
    public async Task ListBarbers_InvalidPageSize_ShouldReturn200AndCorrectToPageSize20()
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
        var response = await _client.GetAsync("/api/barbers?pageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(20); // Should be corrected to 20
    }

    [Fact]
    public async Task ListBarbers_PageSizeTooLarge_ShouldReturn200AndCorrectToPageSize20()
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
        var response = await _client.GetAsync("/api/barbers?pageSize=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(20); // Should be corrected to 20
    }

    [Fact]
    public async Task ListBarbers_NoAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/barbers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListBarbers_InvalidRole_ShouldReturn403()
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
        var response = await _client.GetAsync("/api/barbers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateBarber_EmptyName_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        var updateInput = new UpdateBarberInput(
            Id: createdBarber!.Id,
            Name: "", // Empty name
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{createdBarber.Id}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBarber_NameTooLong_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        var updateInput = new UpdateBarberInput(
            Id: createdBarber!.Id,
            Name: new string('A', 101), // 101 characters, exceeds max of 100
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{createdBarber.Id}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBarber_InvalidPhone_ShouldReturn400()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a barber first
        var createInput = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.silva@test.com",
            Password: "Password123!",
            Phone: "11987654321",
            ServiceIds: new List<Guid> { _testServiceId }
        );
        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();

        var updateInput = new UpdateBarberInput(
            Id: createdBarber!.Id,
            Name: "João Silva Atualizado",
            Phone: "123", // Invalid phone format
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{createdBarber.Id}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBarber_BarberNotFound_ShouldReturn404()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidId = Guid.NewGuid();
        var updateInput = new UpdateBarberInput(
            Id: invalidId,
            Name: "João Silva Atualizado",
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{invalidId}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBarber_NoAuthentication_ShouldReturn401()
    {
        // Arrange
        var updateInput = new UpdateBarberInput(
            Id: Guid.NewGuid(),
            Name: "João Silva Atualizado",
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{Guid.NewGuid()}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateBarber_InvalidRole_ShouldReturn403()
    {
        // Arrange
        var barberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _testBarberId.ToString(),
            userType: "Barbeiro",
            email: "carlos@teste.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", barberToken);

        var updateInput = new UpdateBarberInput(
            Id: Guid.NewGuid(),
            Name: "João Silva Atualizado",
            Phone: "11987654322",
            ServiceIds: new List<Guid> { _testServiceId }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{Guid.NewGuid()}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateBarber_BarberNotFound_ShouldReturn404()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/barbers/{invalidId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateBarber_NoAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.PutAsync($"/api/barbers/{Guid.NewGuid()}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateBarber_InvalidRole_ShouldReturn403()
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
        var response = await _client.PutAsync($"/api/barbers/{Guid.NewGuid()}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveBarber_BarberNotFound_ShouldReturn404()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/barbers/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveBarber_NoAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/barbers/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveBarber_InvalidRole_ShouldReturn403()
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
        var response = await _client.DeleteAsync($"/api/barbers/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ResetBarberPassword_BarberNotFound_ShouldReturn404()
    {
        // Arrange
        var adminToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/barbers/{invalidId}/reset-password", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetBarberPassword_NoAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.PostAsync($"/api/barbers/{Guid.NewGuid()}/reset-password", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetBarberPassword_InvalidRole_ShouldReturn403()
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
        var response = await _client.PostAsync($"/api/barbers/{Guid.NewGuid()}/reset-password", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}