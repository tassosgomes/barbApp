using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class DeleteBarbershopUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteBarbershopUseCase _useCase;

    public DeleteBarbershopUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _useCase = new DeleteBarbershopUseCase(
            _barbershopRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingBarbershop_ShouldDeleteSuccessfully()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("01310100", "Av. Paulista", "1000", "Sala 15", "Bela Vista", "São Paulo", "SP");
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            Document.Create("12345678000190"),
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            UniqueCode.Create("ABCDEFGH"),
            "admin");

        // Set the ID to match our test
        typeof(Barbershop).GetProperty("Id")?.SetValue(barbershop, barbershopId);

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        // Act
        await _useCase.ExecuteAsync(barbershopId, CancellationToken.None);

        // Assert
        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _barbershopRepositoryMock.Verify(x => x.DeleteAsync(barbershop, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistingBarbershop_ShouldThrowBarbershopNotFoundException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarbershopNotFoundException>(
            () => _useCase.ExecuteAsync(barbershopId, CancellationToken.None));

        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _barbershopRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}