using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarbApp.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class ScheduleControllerIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _testBarbeariaId;
    private Guid _testBarberId;
    private Guid _otherBarbeariaId;
    private Guid _otherBarberId;
    private Guid _testCustomerId;
    private Guid _testServiceId;
    private List<Guid> _testAppointmentIds;

    public ScheduleControllerIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = dbFixture.CreateFactory();
        _client = _factory.CreateClient();
        _testAppointmentIds = new List<Guid>();
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.ResetDatabaseAsync();
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

        // Create test barbearia 1
        var address1 = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document1 = Document.Create("12345678000195");
        var code1 = UniqueCode.Create("SCHED234");
        var barbearia1 = Barbershop.Create(
            "Barbearia Teste Schedule",
            document1,
            "(11) 98765-4321",
            "João Silva",
            "joao@teste.com",
            address1,
            code1,
            "test"
        );

        // Create test barbearia 2
        var address2 = Address.Create("01310101", "Av. Brigadeiro", "1001", null, "Jardim Paulista", "São Paulo", "SP");
        var document2 = Document.Create("98765432000189");
        var code2 = UniqueCode.Create("OTHER567");
        var barbearia2 = Barbershop.Create(
            "Barbearia Outra",
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

        // Create barber 1 in barbearia 1
        var barber1 = Barber.Create(
            _testBarbeariaId,
            "Test Barber 1",
            "barber1@test.com",
            "hashedpassword",
            "11987654321"
        );

        // Create barber 2 in barbearia 2
        var barber2 = Barber.Create(
            _otherBarbeariaId,
            "Test Barber 2",
            "barber2@test.com",
            "hashedpassword",
            "11987654322"
        );

        dbContext.Barbers.AddRange(barber1, barber2);
        await dbContext.SaveChangesAsync();

        _testBarberId = barber1.Id;
        _otherBarberId = barber2.Id;

        // Create customer in barbearia 1
        var customer = Customer.Create(_testBarbeariaId, "11999999999", "Cliente Teste");
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        _testCustomerId = customer.Id;

        // Create service in barbearia 1
        var service = BarbershopService.Create(_testBarbeariaId, "Corte Masculino", "Corte completo", 30, 25.00m);
        dbContext.BarbershopServices.Add(service);
        await dbContext.SaveChangesAsync();

        _testServiceId = service.Id;

        // Create appointments for barber 1 on today
        var today = DateTime.UtcNow.Date;
        var appointment1 = Appointment.Create(
            _testBarbeariaId,
            _testBarberId,
            _testCustomerId,
            _testServiceId,
            DateTime.SpecifyKind(today.AddHours(10), DateTimeKind.Utc),
            DateTime.SpecifyKind(today.AddHours(10.5), DateTimeKind.Utc)
        );

        var appointment2 = Appointment.Create(
            _testBarbeariaId,
            _testBarberId,
            _testCustomerId,
            _testServiceId,
            DateTime.SpecifyKind(today.AddHours(14), DateTimeKind.Utc),
            DateTime.SpecifyKind(today.AddHours(14.5), DateTimeKind.Utc)
        );
        appointment2.Confirm();

        var appointment3 = Appointment.Create(
            _testBarbeariaId,
            _testBarberId,
            _testCustomerId,
            _testServiceId,
            DateTime.SpecifyKind(today.AddHours(16), DateTimeKind.Utc),
            DateTime.SpecifyKind(today.AddHours(16.5), DateTimeKind.Utc)
        );
        appointment3.Cancel();

        dbContext.Appointments.AddRange(appointment1, appointment2, appointment3);
        await dbContext.SaveChangesAsync();

        _testAppointmentIds = new List<Guid> { appointment1.Id, appointment2.Id, appointment3.Id };

        // Set JWT token for Barbeiro
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _testBarberId.ToString(),
            userType: "Barbeiro",
            email: "barber1@test.com",
            barbeariaId: _testBarbeariaId
        );
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetMySchedule_ValidDate_ShouldReturn200WithAppointments()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        // Act
        var response = await _client.GetAsync($"/api/schedule/my-schedule?date={today:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BarberScheduleOutput>();
        result.Should().NotBeNull();
        result!.BarberId.Should().Be(_testBarberId);
        result.Appointments.Should().HaveCount(3);
        result.Appointments.Should().Contain(a => a.Status == AppointmentStatus.Pending);
        result.Appointments.Should().Contain(a => a.Status == AppointmentStatus.Confirmed);
        result.Appointments.Should().Contain(a => a.Status == AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task GetMySchedule_NoDate_ShouldReturnTodaySchedule()
    {
        // Act
        var response = await _client.GetAsync("/api/schedule/my-schedule");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BarberScheduleOutput>();
        result.Should().NotBeNull();
        result!.Date.Date.Should().Be(DateTime.UtcNow.Date);
        result.Appointments.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMySchedule_EmptyDate_ShouldReturn200WithEmptyList()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/schedule/my-schedule?date={futureDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BarberScheduleOutput>();
        result.Should().NotBeNull();
        result!.Appointments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMySchedule_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.GetAsync("/api/schedule/my-schedule");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMySchedule_WithWrongRole_ShouldReturn403()
    {
        // Arrange - Use AdminCentral role instead of Barbeiro
        var wrongRoleToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminCentral",
            email: "admin@test.com"
        );

        var clientWithWrongRole = _factory.CreateClient();
        clientWithWrongRole.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", wrongRoleToken);

        // Act
        var response = await clientWithWrongRole.GetAsync("/api/schedule/my-schedule");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMySchedule_DifferentBarbearia_ShouldReturnEmptyList()
    {
        // Arrange - Authenticate as barber from other barbearia
        var otherBarberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _otherBarberId.ToString(),
            userType: "Barbeiro",
            email: "barber2@test.com",
            barbeariaId: _otherBarbeariaId
        );

        var otherBarberClient = _factory.CreateClient();
        otherBarberClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", otherBarberToken);

        var today = DateTime.UtcNow.Date;

        // Act
        var response = await otherBarberClient.GetAsync($"/api/schedule/my-schedule?date={today:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BarberScheduleOutput>();
        result.Should().NotBeNull();
        result!.Appointments.Should().BeEmpty(); // Should not see appointments from other barbearia
    }

    [Fact]
    public async Task GetMySchedule_OrderedByTime_ShouldReturnChronologicalOrder()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        // Act
        var response = await _client.GetAsync($"/api/schedule/my-schedule?date={today:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BarberScheduleOutput>();
        result.Should().NotBeNull();
        
        var appointments = result!.Appointments;
        appointments.Should().HaveCount(3);
        
        // Verify chronological order
        for (int i = 0; i < appointments.Count - 1; i++)
        {
            appointments[i].StartTime.Should().BeBefore(appointments[i + 1].StartTime);
        }
    }
}
