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

public class GetAppointmentDetailsUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IBarbershopServiceRepository> _serviceRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAppointmentDetailsUseCase>> _loggerMock;
    private readonly GetAppointmentDetailsUseCase _useCase;

    public GetAppointmentDetailsUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _serviceRepositoryMock = new Mock<IBarbershopServiceRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAppointmentDetailsUseCase>>();

        _useCase = new GetAppointmentDetailsUseCase(
            _appointmentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidAppointment_ShouldReturnDetails()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var customer = Customer.Create(barbeariaId, "11987654321", "Maria Silva");
        var service = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        var appointment = Appointment.Create(
            barbeariaId, barberId, customer.Id, service.Id,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1).AddMinutes(30));

        // Set the appointment ID
        typeof(Appointment).GetProperty("Id")?.SetValue(appointment, appointmentId);

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _tenantContextMock.Setup(x => x.UserId).Returns(barberId.ToString());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _useCase.ExecuteAsync(appointmentId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(appointmentId);
        result.CustomerName.Should().Be("Maria Silva");
        result.CustomerPhone.Should().Be("11987654321");
        result.ServiceTitle.Should().Be("Corte");
        result.ServicePrice.Should().Be(25.00m);
        result.ServiceDurationMinutes.Should().Be(30);
        result.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public async Task ExecuteAsync_AppointmentNotOwnedByBarber_ShouldThrowForbiddenException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var otherBarberId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var appointment = Appointment.Create(
            barbeariaId, otherBarberId, Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1).AddMinutes(30));

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _tenantContextMock.Setup(x => x.UserId).Returns(barberId.ToString());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _useCase.ExecuteAsync(appointmentId, CancellationToken.None));
    }
}