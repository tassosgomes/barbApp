using BarbApp.Application.Interfaces;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Infrastructure.Tests.Services;

public class UniqueCodeGeneratorTests
{
    private readonly Mock<IBarbershopRepository> _repositoryMock;
    private readonly Mock<ILogger<UniqueCodeGenerator>> _loggerMock;
    private readonly UniqueCodeGenerator _generator;

    public UniqueCodeGeneratorTests()
    {
        _repositoryMock = new Mock<IBarbershopRepository>();
        _loggerMock = new Mock<ILogger<UniqueCodeGenerator>>();
        _generator = new UniqueCodeGenerator(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_NoCollision_ShouldReturnCode()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act
        var code = await _generator.GenerateAsync(CancellationToken.None);

        // Assert
        code.Should().NotBeNullOrEmpty();
        code.Should().HaveLength(8);
        code.Should().MatchRegex("^[A-Z2-9]{8}$");
    }

    [Fact]
    public async Task GenerateAsync_CollisionThenSuccess_ShouldRetryAndReturn()
    {
        // Arrange
        var existingBarbershop = Barbershop.Create(
            "Test",
            Document.Create("12345678000190"),
            "11987654321",
            "Owner",
            "test@test.com",
            Address.Create("01310100", "Street", "100", null, "Neighborhood", "City", "State"),
            UniqueCode.Create("COLLIDE2"),
            "admin");

        var callCount = 0;
        _repositoryMock
            .Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ < 2 ? existingBarbershop : null);

        // Act
        var code = await _generator.GenerateAsync(CancellationToken.None);

        // Assert
        code.Should().NotBeNullOrEmpty();
        code.Should().NotBe("COLLIDE2");
        _repositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GenerateAsync_MaxRetriesExceeded_ShouldThrowException()
    {
        // Arrange
        var existingBarbershop = Barbershop.Create(
            "Test",
            Document.Create("12345678000190"),
            "11987654321",
            "Owner",
            "test@test.com",
            Address.Create("01310100", "Street", "100", null, "Neighborhood", "City", "State"),
            UniqueCode.Create("COLLIDE2"),
            "admin");

        _repositoryMock
            .Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBarbershop);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _generator.GenerateAsync(CancellationToken.None));

        _repositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(10));
    }
}