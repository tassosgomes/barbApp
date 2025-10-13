using BarbApp.Application.DTOs;
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

public class GetBarbershopUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<ILogger<GetBarbershopUseCase>> _loggerMock;
    private readonly GetBarbershopUseCase _useCase;

    public GetBarbershopUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _loggerMock = new Mock<ILogger<GetBarbershopUseCase>>();
        _useCase = new GetBarbershopUseCase(_barbershopRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingBarbershop_ShouldReturnBarbershopOutput()
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
        var result = await _useCase.ExecuteAsync(barbershopId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(barbershopId);
        result.Name.Should().Be("Barbearia Teste");
        result.Document.Should().Be("12345678000190");
        result.Code.Should().Be("ABCDEFGH");
        result.IsActive.Should().BeTrue();

        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
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
    }
}