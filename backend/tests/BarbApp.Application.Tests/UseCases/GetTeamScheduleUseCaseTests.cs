using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class GetTeamScheduleUseCaseTests
{
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IBarberRepository> _barberRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ILogger<GetTeamScheduleUseCase>> _loggerMock;
    private readonly GetTeamScheduleUseCase _useCase;

    public GetTeamScheduleUseCaseTests()
    {
        _tenantContextMock = new Mock<ITenantContext>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _barberRepositoryMock = new Mock<IBarberRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _loggerMock = new Mock<ILogger<GetTeamScheduleUseCase>>();

        _useCase = new GetTeamScheduleUseCase(
            _tenantContextMock.Object,
            _appointmentRepositoryMock.Object,
            _barberRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldReturnTeamSchedule()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var date = DateTime.Today;

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);

        var appointment = CreateAppointment(Guid.NewGuid(), barberId, customerId, date.AddHours(10), date.AddHours(11), "Corte", "Confirmed");
        var appointments = new List<Appointment> { appointment };

        _appointmentRepositoryMock
            .Setup(x => x.GetAppointmentsByBarbeariaAndDateAsync(barbeariaId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        var barber = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        var customer = Customer.Create(barbeariaId, "11987654321", "Maria Santos");
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _useCase.ExecuteAsync(date, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Appointments.Should().HaveCount(1);
        var appointmentOutput = result.Appointments.First();
        appointmentOutput.BarberName.Should().Be("João Silva");
        appointmentOutput.CustomerName.Should().Be("Maria Santos");
        appointmentOutput.ServiceName.Should().Be("Corte");
        appointmentOutput.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task ExecuteAsync_WithBarberFilter_ShouldFilterAppointments()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId1 = Guid.NewGuid();
        var barberId2 = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var date = DateTime.Today;

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);

        var appointment1 = CreateAppointment(Guid.NewGuid(), barberId1, customerId, date.AddHours(10), date.AddHours(11), "Corte", "Confirmed");
        var appointment2 = CreateAppointment(Guid.NewGuid(), barberId2, customerId, date.AddHours(14), date.AddHours(15), "Barba", "Confirmed");
        var appointments = new List<Appointment> { appointment1, appointment2 };

        _appointmentRepositoryMock
            .Setup(x => x.GetAppointmentsByBarbeariaAndDateAsync(barbeariaId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        var barber1 = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());
        var barber2 = Barber.Create(barbeariaId, "Pedro Santos", "pedro@test.com", "hash", "11987654322", new List<Guid>());
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber1);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber2);

        var customer = Customer.Create(barbeariaId, "11987654321", "Maria Santos");
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _useCase.ExecuteAsync(date, barberId1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Appointments.Should().HaveCount(1);
        var appointmentOutput = result.Appointments.First();
        appointmentOutput.BarberId.Should().Be(barberId1);
        appointmentOutput.BarberName.Should().Be("João Silva");
    }

    [Fact]
    public async Task ExecuteAsync_NoTenantContext_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _tenantContextMock.Setup(x => x.BarbeariaId).Returns((Guid?)null);

        // Act
        var act = async () => await _useCase.ExecuteAsync(DateTime.Today, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Contexto de barbearia não definido");
    }

    [Fact]
    public async Task ExecuteAsync_BarberNotFound_ShouldUseUnknownBarber()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var date = DateTime.Today;

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);

        var appointment = CreateAppointment(Guid.NewGuid(), barberId, customerId, date.AddHours(10), date.AddHours(11), "Corte", "Confirmed");
        var appointments = new List<Appointment> { appointment };

        _appointmentRepositoryMock
            .Setup(x => x.GetAppointmentsByBarbeariaAndDateAsync(barbeariaId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        var customer = Customer.Create(barbeariaId, "11987654321", "Maria Santos");
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _useCase.ExecuteAsync(date, null, CancellationToken.None);

        // Assert
        result.Appointments.Should().HaveCount(1);
        result.Appointments.First().BarberName.Should().Be("Unknown Barber");
    }

    [Fact]
    public async Task ExecuteAsync_CustomerNotFound_ShouldUseUnknownCustomer()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var date = DateTime.Today;

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);

        var appointment = CreateAppointment(Guid.NewGuid(), barberId, customerId, date.AddHours(10), date.AddHours(11), "Corte", "Confirmed");
        var appointments = new List<Appointment> { appointment };

        _appointmentRepositoryMock
            .Setup(x => x.GetAppointmentsByBarbeariaAndDateAsync(barbeariaId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        var barber = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _useCase.ExecuteAsync(date, null, CancellationToken.None);

        // Assert
        result.Appointments.Should().HaveCount(1);
        result.Appointments.First().CustomerName.Should().Be("Unknown Customer");
    }

    private static Appointment CreateAppointment(Guid id, Guid barberId, Guid customerId, DateTime startTime, DateTime endTime, string serviceName, string status)
    {
        var appointment = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        typeof(Appointment).GetProperty("Id")!.SetValue(appointment, id);
        typeof(Appointment).GetProperty("BarberId")!.SetValue(appointment, barberId);
        typeof(Appointment).GetProperty("CustomerId")!.SetValue(appointment, customerId);
        typeof(Appointment).GetProperty("StartTime")!.SetValue(appointment, startTime);
        typeof(Appointment).GetProperty("EndTime")!.SetValue(appointment, endTime);
        typeof(Appointment).GetProperty("ServiceName")!.SetValue(appointment, serviceName);
        typeof(Appointment).GetProperty("Status")!.SetValue(appointment, status);
        return appointment;
    }
}