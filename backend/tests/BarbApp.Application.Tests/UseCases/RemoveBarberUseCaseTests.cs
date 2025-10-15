using BarbApp.Application.Interfaces;
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

public class RemoveBarberUseCaseTests
{
    private readonly Mock<IBarberRepository> _barberRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RemoveBarberUseCase>> _loggerMock;
    private readonly RemoveBarberUseCase _useCase;

    public RemoveBarberUseCaseTests()
    {
        _barberRepositoryMock = new Mock<IBarberRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RemoveBarberUseCase>>();

        _useCase = new RemoveBarberUseCase(
            _barberRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidBarberWithFutureAppointments_ShouldCancelAppointmentsAndDeactivateBarber()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var barber = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());
        
        // Create mock appointments using reflection to set private properties
        var appointment1 = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        typeof(Appointment).GetProperty("Id")!.SetValue(appointment1, Guid.NewGuid());
        typeof(Appointment).GetProperty("BarberId")!.SetValue(appointment1, barberId);
        typeof(Appointment).GetProperty("Status")!.SetValue(appointment1, "Confirmed");
        
        var appointment2 = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        typeof(Appointment).GetProperty("Id")!.SetValue(appointment2, Guid.NewGuid());
        typeof(Appointment).GetProperty("BarberId")!.SetValue(appointment2, barberId);
        typeof(Appointment).GetProperty("Status")!.SetValue(appointment2, "Pending");
        
        var futureAppointments = new List<Appointment> { appointment1, appointment2 };

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);
        _appointmentRepositoryMock
            .Setup(x => x.GetFutureAppointmentsByBarberAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureAppointments);

        // Act
        await _useCase.ExecuteAsync(barberId, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(x => x.UpdateStatusAsync(futureAppointments, "Cancelled", It.IsAny<CancellationToken>()), Times.Once);
        _barberRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Barber>(b => !b.IsActive), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidBarberWithNoFutureAppointments_ShouldOnlyDeactivateBarber()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var barber = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());
        var futureAppointments = new List<Appointment>();

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);
        _appointmentRepositoryMock
            .Setup(x => x.GetFutureAppointmentsByBarberAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureAppointments);

        // Act
        await _useCase.ExecuteAsync(barberId, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(x => x.UpdateStatusAsync(It.IsAny<IEnumerable<Appointment>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _barberRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Barber>(b => !b.IsActive), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }    [Fact]
    public async Task ExecuteAsync_BarberNotFound_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarberNotFoundException>(
            () => _useCase.ExecuteAsync(barberId, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_BarberFromDifferentBarbearia_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var otherBarbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var barber = Barber.Create(otherBarbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        // Act & Assert
        await Assert.ThrowsAsync<BarberNotFoundException>(
            () => _useCase.ExecuteAsync(barberId, CancellationToken.None));
    }
}