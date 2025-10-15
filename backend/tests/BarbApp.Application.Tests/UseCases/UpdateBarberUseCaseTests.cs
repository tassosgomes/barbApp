using BarbApp.Application.DTOs;
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

public class UpdateBarberUseCaseTests
{
    private readonly Mock<IBarberRepository> _barberRepositoryMock;
    private readonly Mock<IBarbershopServiceRepository> _serviceRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateBarberUseCase>> _loggerMock;
    private readonly UpdateBarberUseCase _useCase;

    public UpdateBarberUseCaseTests()
    {
        _barberRepositoryMock = new Mock<IBarberRepository>();
        _serviceRepositoryMock = new Mock<IBarbershopServiceRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateBarberUseCase>>();

        _useCase = new UpdateBarberUseCase(
            _barberRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ShouldUpdateBarber()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var input = new UpdateBarberInput(
            barberId,
            "João Silva Atualizado",
            "11987654322",
            new List<Guid> { serviceId });

        var barber = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);
        _serviceRepositoryMock
            .Setup(x => x.ListAsync(barbeariaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService>
            {
                BarbershopService.Create(barbeariaId, "Corte", null, 30, 25.00m)
            });

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("João Silva Atualizado");
        result.PhoneFormatted.Should().Be("(11) 98765-4322");

        _barberRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_BarberNotFound_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var input = new UpdateBarberInput(
            barberId,
            "João Silva",
            "11987654321",
            new List<Guid>());

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarberNotFoundException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_BarberFromDifferentBarbearia_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var otherBarbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var input = new UpdateBarberInput(
            barberId,
            "João Silva",
            "11987654321",
            new List<Guid>());

        var barber = Barber.Create(otherBarbeariaId, "João Silva", "joao@test.com", "hash", "11987654321", new List<Guid>());

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        // Act & Assert
        await Assert.ThrowsAsync<BarberNotFoundException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));
    }
}