using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class GetBarberScheduleUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IBarberRepository> _barberRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IBarbershopServiceRepository> _serviceRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetBarberScheduleUseCase>> _loggerMock;
    private readonly GetBarberScheduleUseCase _useCase;

    public GetBarberScheduleUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _barberRepositoryMock = new Mock<IBarberRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _serviceRepositoryMock = new Mock<IBarbershopServiceRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetBarberScheduleUseCase>>();

        _useCase = new GetBarberScheduleUseCase(
            _appointmentRepositoryMock.Object,
            _barberRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldReturnBarberScheduleOutput()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var barber = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hashedpassword", "11987654321");

        // Set barber ID
        typeof(Barber).GetProperty("Id")?.SetValue(barber, barberId);

        var customer = Customer.Create(barbeariaId, "11987654321", "Maria Silva");
        var service = BarbershopService.Create(barbeariaId, "Corte de Cabelo", "Corte masculino", 30, 25.00m);

        var appointment = Appointment.Create(
            barbeariaId, barberId, customer.Id, service.Id,
            date.AddHours(10), date.AddHours(10).AddMinutes(30));

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _tenantContextMock.Setup(x => x.UserId).Returns(barberId.ToString());

        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        _appointmentRepositoryMock
            .Setup(x => x.GetByBarberAndDateAsync(barbeariaId, barberId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { appointment });

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _useCase.ExecuteAsync(date, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.BarberId.Should().Be(barberId);
        result.BarberName.Should().Be("João Silva");
        result.Appointments.Should().HaveCount(1);

        var apt = result.Appointments.First();
        apt.Id.Should().Be(appointment.Id);
        apt.CustomerName.Should().Be("Maria Silva");
        apt.ServiceTitle.Should().Be("Corte de Cabelo");
        apt.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public async Task ExecuteAsync_BarberNotFound_ShouldThrowBarberNotFoundException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _tenantContextMock.Setup(x => x.UserId).Returns(barberId.ToString());

        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarberNotFoundException>(
            () => _useCase.ExecuteAsync(date, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_NoBarbeariaContext_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns((Guid?)null);
        _tenantContextMock.Setup(x => x.UserId).Returns(barberId.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>(
            () => _useCase.ExecuteAsync(date, CancellationToken.None));
    }
}