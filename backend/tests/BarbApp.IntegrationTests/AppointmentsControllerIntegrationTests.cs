using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarbApp.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AppointmentsControllerIntegrationTests : IAsyncLifetime
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

    public AppointmentsControllerIntegrationTests(DatabaseFixture dbFixture)
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

        // Create test barbearia 1
        var address1 = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document1 = Document.Create("11111111000191");
        var code1 = UniqueCode.Create("APPTS234");
        var barbearia1 = Barbershop.Create(
            "Barbearia Teste Appointments",
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
        var document2 = Document.Create("22222222000192");
        var code2 = UniqueCode.Create("APPTS567");
        var barbearia2 = Barbershop.Create(
            "Barbearia Outra Appointments",
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

    #region GetAppointmentDetails Tests

    [Fact]
    public async Task GetAppointmentDetails_ValidId_ShouldReturn200WithDetails()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);

        // Act
        var response = await _client.GetAsync($"/api/appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AppointmentDetailsOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
        result.CustomerName.Should().Be("Cliente Teste");
        result.ServiceTitle.Should().Be("Corte Masculino");
        result.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public async Task GetAppointmentDetails_NonExistentId_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/appointments/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAppointmentDetails_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);
        var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.GetAsync($"/api/appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAppointmentDetails_WithWrongRole_ShouldReturn403()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);
        
        var wrongRoleToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminCentral",
            email: "admin@test.com"
        );

        var clientWithWrongRole = _factory.CreateClient();
        clientWithWrongRole.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", wrongRoleToken);

        // Act
        var response = await clientWithWrongRole.GetAsync($"/api/appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentDetails_FromOtherBarbearia_ShouldReturn403()
    {
        // Arrange - Create appointment for barber in other barbearia
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);

        // Authenticate as barber from different barbearia
        var otherBarberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _otherBarberId.ToString(),
            userType: "Barbeiro",
            email: "barber2@test.com",
            barbeariaId: _otherBarbeariaId
        );

        var otherBarberClient = _factory.CreateClient();
        otherBarberClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", otherBarberToken);

        // Act
        var response = await otherBarberClient.GetAsync($"/api/appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden); // Tenant context prevents access
    }

    #endregion

    #region ConfirmAppointment Tests

    [Fact]
    public async Task ConfirmAppointment_PendingStatus_ShouldReturn200AndConfirm()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AppointmentDetailsOutput>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(AppointmentStatus.Confirmed);
        result.ConfirmedAt.Should().NotBeNull();

        // Verify in database
        var dbAppointment = await GetAppointmentFromDb(appointment.Id);
        dbAppointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmAppointment_AlreadyConfirmed_ShouldReturn409()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Confirmed);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConfirmAppointment_CancelledStatus_ShouldReturn409()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Cancelled);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConfirmAppointment_CompletedStatus_ShouldReturn409()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Completed);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConfirmAppointment_NonExistent_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/appointments/{nonExistentId}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmAppointment_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);
        var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConfirmAppointment_FromOtherBarbearia_ShouldReturn403()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);

        var otherBarberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _otherBarberId.ToString(),
            userType: "Barbeiro",
            email: "barber2@test.com",
            barbeariaId: _otherBarbeariaId
        );

        var otherBarberClient = _factory.CreateClient();
        otherBarberClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", otherBarberToken);

        // Act
        var response = await otherBarberClient.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region CancelAppointment Tests

    [Fact]
    public async Task CancelAppointment_PendingStatus_ShouldReturn200AndCancel()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AppointmentDetailsOutput>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(AppointmentStatus.Cancelled);
        result.CancelledAt.Should().NotBeNull();

        // Verify in database
        var dbAppointment = await GetAppointmentFromDb(appointment.Id);
        dbAppointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAppointment_ConfirmedStatus_ShouldReturn200AndCancel()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Confirmed);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AppointmentDetailsOutput>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAppointment_AlreadyCancelled_ShouldReturn409()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Cancelled);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelAppointment_CompletedStatus_ShouldReturn409()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Completed);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelAppointment_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);
        var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.PostAsync($"/api/appointments/{appointment.Id}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelAppointment_FromOtherBarbearia_ShouldReturn403()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending);

        var otherBarberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _otherBarberId.ToString(),
            userType: "Barbeiro",
            email: "barber2@test.com",
            barbeariaId: _otherBarbeariaId
        );

        var otherBarberClient = _factory.CreateClient();
        otherBarberClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", otherBarberToken);

        // Act
        var response = await otherBarberClient.PostAsync($"/api/appointments/{appointment.Id}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region CompleteAppointment Tests

    [Fact]
    public async Task CompleteAppointment_ConfirmedAndPastTime_ShouldReturn200AndComplete()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddHours(-2);
        var appointment = await CreateTestAppointment(AppointmentStatus.Confirmed, pastTime);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AppointmentDetailsOutput>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(AppointmentStatus.Completed);
        result.CompletedAt.Should().NotBeNull();

        // Verify in database
        var dbAppointment = await GetAppointmentFromDb(appointment.Id);
        dbAppointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public async Task CompleteAppointment_PendingStatus_ShouldReturn409()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddHours(-2);
        var appointment = await CreateTestAppointment(AppointmentStatus.Pending, pastTime);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompleteAppointment_FutureTime_ShouldReturn400()
    {
        // Arrange
        var futureTime = DateTime.UtcNow.AddHours(2);
        var appointment = await CreateTestAppointment(AppointmentStatus.Confirmed, futureTime);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteAppointment_AlreadyCompleted_ShouldReturn409()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Completed);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompleteAppointment_CancelledStatus_ShouldReturn409()
    {
        // Arrange
        var appointment = await CreateTestAppointment(AppointmentStatus.Cancelled);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompleteAppointment_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddHours(-2);
        var appointment = await CreateTestAppointment(AppointmentStatus.Confirmed, pastTime);
        var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CompleteAppointment_FromOtherBarbearia_ShouldReturn403()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddHours(-2);
        var appointment = await CreateTestAppointment(AppointmentStatus.Confirmed, pastTime);

        var otherBarberToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: _otherBarberId.ToString(),
            userType: "Barbeiro",
            email: "barber2@test.com",
            barbeariaId: _otherBarbeariaId
        );

        var otherBarberClient = _factory.CreateClient();
        otherBarberClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", otherBarberToken);

        // Act
        var response = await otherBarberClient.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper Methods

    private async Task<Appointment> CreateTestAppointment(AppointmentStatus status, DateTime? startTime = null)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        // For completed status, we need a past time
        DateTime start;
        if (status == AppointmentStatus.Completed && startTime == null)
        {
            start = DateTime.UtcNow.AddHours(-2);
        }
        else
        {
            start = startTime ?? DateTime.UtcNow.AddHours(2);
        }
        
        var end = start.AddMinutes(30);

        // Ensure times are UTC
        start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        var appointment = Appointment.Create(
            _testBarbeariaId,
            _testBarberId,
            _testCustomerId,
            _testServiceId,
            start,
            end
        );

        // Apply status transitions
        switch (status)
        {
            case AppointmentStatus.Confirmed:
                appointment.Confirm();
                break;
            case AppointmentStatus.Cancelled:
                appointment.Cancel();
                break;
            case AppointmentStatus.Completed:
                appointment.Confirm();
                appointment.Complete();
                break;
        }

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync();

        return appointment;
    }

    private async Task<Appointment> GetAppointmentFromDb(Guid id)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var appointment = await dbContext.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment!;
    }

    #endregion
}
