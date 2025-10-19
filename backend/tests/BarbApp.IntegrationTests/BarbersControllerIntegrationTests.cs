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
public class BarbersControllerIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _testBarbeariaId;
    private Guid _otherBarbeariaId;
    private List<Guid> _testServiceIds;

    public BarbersControllerIntegrationTests(DatabaseFixture dbFixture)
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
    public async Task CreateBarber_ValidData_ShouldReturn201AndCreateBarber()
    {
        // Arrange
        var input = new CreateBarberInput(
            Name: "Carlos Santos",
            Email: "carlos@teste.com",
            Password: "Password123!",
            Phone: "(11) 98765-4323",
            ServiceIds: _testServiceIds
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<BarberOutput>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Carlos Santos");
        result.Email.Should().Be("carlos@teste.com");
        result.PhoneFormatted.Should().Be("(11) 98765-4323");
        result.Services.Should().HaveCount(3);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task CreateBarber_DuplicateEmailInSameBarbearia_ShouldReturn409()
    {
        // Arrange - Create first barber
        var input1 = new CreateBarberInput(
            Name: "João Silva",
            Email: "joao.duplicate@teste.com",
            Password: "Password123!",
            Phone: "(11) 98765-4324",
            ServiceIds: _testServiceIds.Take(1).ToList()
        );

        var response1 = await _client.PostAsJsonAsync("/api/barbers", input1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create second barber with same email in same barbearia
        var input2 = new CreateBarberInput(
            Name: "João Silva Jr",
            Email: "joao.duplicate@teste.com", // Same email
            Password: "Password456!",
            Phone: "(11) 98765-4325",
            ServiceIds: _testServiceIds.Take(2).ToList()
        );

        // Act
        var response2 = await _client.PostAsJsonAsync("/api/barbers", input2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateBarber_SameEmailInDifferentBarbearia_ShouldReturn201()
    {
        // Arrange - Create barber in first barbearia
        var input1 = new CreateBarberInput(
            Name: "Pedro Oliveira",
            Email: "pedro@teste.com",
            Password: "Password123!",
            Phone: "(11) 98765-4326",
            ServiceIds: _testServiceIds.Take(1).ToList()
        );

        var response1 = await _client.PostAsJsonAsync("/api/barbers", input1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Create barber with same email in different barbearia
        var tokenForOtherBarbearia = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin2@barbearia.com",
            barbeariaId: _otherBarbeariaId
        );

        var client2 = _factory.CreateClient();
        client2.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenForOtherBarbearia);

        var input2 = new CreateBarberInput(
            Name: "Pedro Oliveira",
            Email: "pedro@teste.com", // Same email, different barbearia
            Password: "Password123!",
            Phone: "(11) 98765-4326",
            ServiceIds: new List<Guid>() // No services for simplicity
        );

        // Act
        var response2 = await client2.PostAsJsonAsync("/api/barbers", input2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ListBarbers_NoFilters_ShouldReturnBarbersFromCurrentBarbearia()
    {
        // Arrange - Create multiple barbers
        var barbers = new[]
        {
            new CreateBarberInput("Ana Costa", "ana@teste.com", "Password123!", "(11) 98765-4327", _testServiceIds.Take(1).ToList()),
            new CreateBarberInput("Bruno Lima", "bruno@teste.com", "Password123!", "(11) 98765-4328", _testServiceIds.Take(2).ToList()),
            new CreateBarberInput("Carla Souza", "carla@teste.com", "Password123!", "(11) 98765-4329", _testServiceIds)
        };

        foreach (var barber in barbers)
        {
            var response = await _client.PostAsJsonAsync("/api/barbers", barber);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var listResponse = await _client.GetAsync("/api/barbers?page=1&pageSize=10");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.Barbers.Should().HaveCount(c => c >= 3);
        result.Barbers.Should().AllSatisfy(barber =>
        {
            barber.Id.Should().NotBeEmpty();
            barber.Name.Should().NotBeNullOrEmpty();
            barber.Email.Should().NotBeNullOrEmpty();
            barber.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task ListBarbers_WithSearchName_ShouldFilterResults()
    {
        // Arrange - Create barbers with different names
        var barber1 = new CreateBarberInput("Marcos Silva", "marcos@teste.com", "Password123!", "(11) 98765-4330", _testServiceIds.Take(1).ToList());
        var barber2 = new CreateBarberInput("Marcia Oliveira", "marcia@teste.com", "Password123!", "(11) 98765-4331", _testServiceIds.Take(2).ToList());

        await _client.PostAsJsonAsync("/api/barbers", barber1);
        await _client.PostAsJsonAsync("/api/barbers", barber2);

        // Act - Search for "Marcos"
        var searchResponse = await _client.GetAsync("/api/barbers?searchName=Marcos&page=1&pageSize=10");

        // Assert
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await searchResponse.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.Barbers.Should().ContainSingle();
        result.Barbers[0].Name.Should().Be("Marcos Silva");
    }

    [Fact]
    public async Task GetBarberById_ExistingId_ShouldReturn200AndBarber()
    {
        // Arrange - Create a barber first
        var createInput = new CreateBarberInput(
            "Diego Santos",
            "diego@teste.com",
            "Password123!",
            "(11) 98765-4332",
            _testServiceIds
        );

        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();
        createdBarber.Should().NotBeNull();

        // Act
        var getResponse = await _client.GetAsync($"/api/barbers/{createdBarber!.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<BarberOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBarber.Id);
        result.Name.Should().Be("Diego Santos");
        result.Email.Should().Be("diego@teste.com");
        result.Services.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetBarberById_NonExistingId_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync($"/api/barbers/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBarber_ExistingId_ShouldReturn200AndUpdateBarber()
    {
        // Arrange - Create a barber first
        var createInput = new CreateBarberInput(
            "Eduardo Lima",
            "eduardo@teste.com",
            "Password123!",
            "(11) 98765-4333",
            _testServiceIds.Take(2).ToList()
        );

        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();
        createdBarber.Should().NotBeNull();

        var updateInput = new UpdateBarberInput(
            Id: createdBarber!.Id,
            Name: "Eduardo Lima Atualizado",
            Phone: "(11) 98765-4334",
            ServiceIds: _testServiceIds.Take(1).ToList()
        );

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/barbers/{createdBarber.Id}", updateInput);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await updateResponse.Content.ReadFromJsonAsync<BarberOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBarber.Id);
        result.Name.Should().Be("Eduardo Lima Atualizado");
        result.PhoneFormatted.Should().Be("(11) 98765-4334");
        result!.Services.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateBarber_NonExistingId_ShouldReturn404()
    {
        // Arrange
        var updateInput = new UpdateBarberInput(
            Id: Guid.NewGuid(),
            Name: "Barbeiro Inexistente",
            Phone: "(11) 98765-4335",
            ServiceIds: new List<Guid>()
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbers/{updateInput.Id}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveBarber_ExistingId_ShouldReturn204AndDeactivate()
    {
        // Arrange - Create a barber first
        var createInput = new CreateBarberInput(
            "Fernando Costa",
            "fernando@teste.com",
            "Password123!",
            "(11) 98765-4336",
            _testServiceIds.Take(1).ToList()
        );

        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();
        createdBarber.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/barbers/{createdBarber!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify barber is deactivated (not returned in list by default)
        var listResponse = await _client.GetAsync("/api/barbers?page=1&pageSize=100");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        listResult.Should().NotBeNull();
        listResult!.Barbers.Should().NotContain(barber => barber.Id == createdBarber.Id);
    }

    [Fact]
    public async Task RemoveBarber_WithFutureAppointments_ShouldCancelAppointments()
    {
        // Arrange - Create a barber and future appointments
        var createInput = new CreateBarberInput(
            "Gabriel Rocha",
            "gabriel@teste.com",
            "Password123!",
            "(11) 98765-4337",
            _testServiceIds.Take(1).ToList()
        );

        var createResponse = await _client.PostAsJsonAsync("/api/barbers", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBarber = await createResponse.Content.ReadFromJsonAsync<BarberOutput>();
        createdBarber.Should().NotBeNull();

        // Create future appointments for this barber
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var customer = Customer.Create(_testBarbeariaId, "(11) 99999-9999", "Cliente Teste");
        dbContext.Customers.Add(customer);

        var appointment1 = Appointment.Create(
            _testBarbeariaId,
            createdBarber!.Id,
            customer.Id,
            _testServiceIds[0],
            DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            DateTime.UtcNow.AddDays(1).Date.AddHours(10).AddMinutes(30)
        );

        var appointment2 = Appointment.Create(
            _testBarbeariaId,
            createdBarber.Id,
            customer.Id,
            _testServiceIds[0],
            DateTime.UtcNow.AddDays(2).Date.AddHours(14),
            DateTime.UtcNow.AddDays(2).Date.AddHours(14).AddMinutes(30)
        );
        
        // Confirm the second appointment
        appointment2.Confirm();

        dbContext.Appointments.AddRange(appointment1, appointment2);
        await dbContext.SaveChangesAsync();

        // Act - Remove barber
        var deleteResponse = await _client.DeleteAsync($"/api/barbers/{createdBarber.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify appointments are cancelled
        dbContext.ChangeTracker.Clear(); // Clear change tracker to ensure fresh query
        var cancelledAppointments = await dbContext.Appointments
            .Where(a => a.BarberId == createdBarber.Id)
            .ToListAsync();

        cancelledAppointments.Should().HaveCount(2);
        cancelledAppointments.Should().AllSatisfy(appointment =>
            appointment.Status.Should().Be(Domain.Enums.AppointmentStatus.Cancelled));
    }

    [Fact]
    public async Task GetTeamSchedule_NoFilters_ShouldReturnTodaysSchedule()
    {
        // Arrange - Create barbers and appointments
        var barberInput = new CreateBarberInput(
            "Henrique Alves",
            "henrique@teste.com",
            "Password123!",
            "(11) 98765-4338",
            _testServiceIds.Take(1).ToList()
        );

        var barberResponse = await _client.PostAsJsonAsync("/api/barbers", barberInput);
        barberResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var barber = await barberResponse.Content.ReadFromJsonAsync<BarberOutput>();
        barber.Should().NotBeNull();

        // Create appointment for today
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var customer = Customer.Create(_testBarbeariaId, "(11) 99999-9998", "Cliente Hoje");
        dbContext.Customers.Add(customer);

        var todayAppointment = Appointment.Create(
            _testBarbeariaId,
            barber!.Id,
            customer.Id,
            _testServiceIds[0],
            DateTime.UtcNow.Date.AddHours(9),
            DateTime.UtcNow.Date.AddHours(9).AddMinutes(30)
        );
        
        // Confirm the appointment
        todayAppointment.Confirm();

        dbContext.Appointments.Add(todayAppointment);
        await dbContext.SaveChangesAsync();

        // Act
        var scheduleResponse = await _client.GetAsync("/api/barbers/schedule");

        // Assert
        scheduleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await scheduleResponse.Content.ReadFromJsonAsync<TeamScheduleOutput>();
        result.Should().NotBeNull();
        result!.Appointments.Should().NotBeEmpty();
        result.Appointments.Should().ContainSingle();
        result.Appointments[0].BarberName.Should().Be("Henrique Alves");
        result.Appointments[0].CustomerName.Should().Be("Cliente Hoje");
        result.Appointments[0].Status.Should().Be("Confirmado");
    }

    [Fact]
    public async Task AccessBarbers_WithoutAuth_ShouldReturn401()
    {
        // Arrange - Create client without auth
        var unauthClient = _factory.CreateClient();

        var input = new CreateBarberInput(
            "Sem Auth",
            "semauth@teste.com",
            "Password123!",
            "(11) 98765-4339",
            new List<Guid>()
        );

        // Act
        var response = await unauthClient.PostAsJsonAsync("/api/barbers", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessBarbers_WithWrongRole_ShouldReturn403()
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

        var input = new CreateBarberInput(
            "Acesso Negado",
            "negado@teste.com",
            "Password123!",
            "(11) 98765-4340",
            new List<Guid>()
        );

        // Act
        var response = await clientWithWrongRole.PostAsJsonAsync("/api/barbers", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AccessBarbers_FromOtherBarbearia_ShouldReturnEmptyList()
    {
        // Arrange - Create barber in test barbearia
        var barberInput = new CreateBarberInput(
            "Barbeiro Teste",
            "barbeiro@teste.com",
            "Password123!",
            "(11) 98765-4341",
            _testServiceIds.Take(1).ToList()
        );

        var createResponse = await _client.PostAsJsonAsync("/api/barbers", barberInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

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

        // Act - Try to list barbers from other barbearia
        var listResponse = await clientForOtherBarbearia.GetAsync("/api/barbers?page=1&pageSize=10");

        // Assert - Should return empty list (global query filter prevents access)
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbersOutput>();
        result.Should().NotBeNull();
        result!.Barbers.Should().BeEmpty();
    }
}