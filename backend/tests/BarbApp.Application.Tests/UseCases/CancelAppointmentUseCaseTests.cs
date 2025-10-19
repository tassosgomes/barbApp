using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
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

public class CancelAppointmentUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IBarbershopServiceRepository> _serviceRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CancelAppointmentUseCase>> _loggerMock;
    private readonly CancelAppointmentUseCase _useCase;

    public CancelAppointmentUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _serviceRepositoryMock = new Mock<IBarbershopServiceRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CancelAppointmentUseCase>>();

        _useCase = new CancelAppointmentUseCase(
            _appointmentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidAppointment_ShouldCancelAndReturnDetails()
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
        result.Status.Should().Be(AppointmentStatus.Cancelled);
        result.CancelledAt.Should().NotBeNull();

        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }
}