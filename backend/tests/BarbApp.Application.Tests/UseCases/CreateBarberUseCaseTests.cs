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

public class CreateBarberUseCaseTests
{
    private readonly Mock<IBarberRepository> _barberRepositoryMock;
    private readonly Mock<IBarbershopServiceRepository> _serviceRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateBarberUseCase>> _loggerMock;
    private readonly CreateBarberUseCase _useCase;

    public CreateBarberUseCaseTests()
    {
        _barberRepositoryMock = new Mock<IBarberRepository>();
        _serviceRepositoryMock = new Mock<IBarbershopServiceRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateBarberUseCase>>();

        _useCase = new CreateBarberUseCase(
            _barberRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _tenantContextMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ShouldCreateBarber()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var input = new CreateBarberInput(
            "João Silva",
            "joao@test.com",
            "Password123!",
            "11987654321",
            new List<Guid> { serviceId });

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByEmailAsync(barbeariaId, "joao@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);
        _passwordHasherMock.Setup(x => x.Hash("Password123!")).Returns("hashed_password");
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
        result.Name.Should().Be("João Silva");
        result.Email.Should().Be("joao@test.com");
        result.PhoneFormatted.Should().Be("(11) 98765-4321");
        result.IsActive.Should().BeTrue();

        _barberRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var input = new CreateBarberInput(
            "João Silva",
            "joao@test.com",
            "Password123!",
            "11987654321",
            new List<Guid>());

        var existingBarber = Barber.Create(barbeariaId, "Existing", "joao@test.com", "hash", "11987654321", new List<Guid>());

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);
        _barberRepositoryMock
            .Setup(x => x.GetByEmailAsync(barbeariaId, "joao@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBarber);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateBarberException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_NoTenantContext_ShouldThrowException()
    {
        // Arrange
        var input = new CreateBarberInput(
            "João Silva",
            "joao@test.com",
            "Password123!",
            "11987654321",
            new List<Guid>());

        _tenantContextMock.Setup(x => x.BarbeariaId).Returns((Guid?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));
    }
}