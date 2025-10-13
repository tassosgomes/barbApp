using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class UpdateBarbershopUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateBarbershopUseCase>> _loggerMock;
    private readonly UpdateBarbershopUseCase _useCase;

    public UpdateBarbershopUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateBarbershopUseCase>>();

        _useCase = new UpdateBarbershopUseCase(
            _barbershopRepositoryMock.Object,
            _addressRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ShouldUpdateBarbershop()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("01310100", "Av. Paulista", "1000", "Sala 15", "Bela Vista", "São Paulo", "SP");
        var barbershop = Barbershop.Create(
            "Barbearia Original",
            Document.Create("12345678000190"),
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            UniqueCode.Create("ABCDEFGH"),
            "admin");

        // Set the ID to match our test
        typeof(Barbershop).GetProperty("Id")?.SetValue(barbershop, barbershopId);

        var input = new UpdateBarbershopInput(
            barbershopId,
            "Barbearia Atualizada",
            "(11) 99999-9999",
            "Maria Silva",
            "maria@test.com",
            "01310-200",
            "Rua Nova",
            "200",
            "Apto 10",
            "Centro",
            "São Paulo",
            "SP");

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(barbershopId);
        result.Name.Should().Be("Barbearia Atualizada");
        result.Phone.Should().Be("11999999999");
        result.OwnerName.Should().Be("Maria Silva");
        result.Email.Should().Be("maria@test.com");
        result.Address.ZipCode.Should().Be("01310200");
        result.Address.Street.Should().Be("Rua Nova");
        result.Address.Number.Should().Be("200");
        result.Address.Complement.Should().Be("Apto 10");
        result.Address.Neighborhood.Should().Be("Centro");

        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _barbershopRepositoryMock.Verify(x => x.UpdateAsync(barbershop, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistingBarbershop_ShouldThrowBarbershopNotFoundException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var input = new UpdateBarbershopInput(
            barbershopId,
            "Barbearia Atualizada",
            "(11) 99999-9999",
            "Maria Silva",
            "maria@test.com",
            "01310-200",
            "Rua Nova",
            "200",
            null,
            "Centro",
            "São Paulo",
            "SP");

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarbershopNotFoundException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));

        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _barbershopRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}