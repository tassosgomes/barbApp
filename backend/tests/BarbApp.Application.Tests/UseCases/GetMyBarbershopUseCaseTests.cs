using BarbApp.Application.DTOs;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class GetMyBarbershopUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly GetMyBarbershopUseCase _useCase;

    public GetMyBarbershopUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _useCase = new GetMyBarbershopUseCase(
            _barbershopRepositoryMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ValidBarbeariaId_ShouldReturnBarbershopOutput()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var address = Address.Create("01310100", "Av. Paulista", "1000", "Sala 15", "Bela Vista", "São Paulo", "SP");
        var barbershop = Barbershop.Create(
            "Barbearia do Tasso Zé",
            Document.Create("12345678000190"),
            "(11) 98765-4321",
            "Tasso Gomes",
            "contato@barbeariatasso.com",
            address,
            UniqueCode.Create("6SJJRFPD"),
            "admin-user-id"
        );

        // Set the ID to match our test
        typeof(Barbershop).GetProperty("Id")?.SetValue(barbershop, barbeariaId);

        _tenantContextMock
            .Setup(x => x.BarbeariaId)
            .Returns(barbeariaId);

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        // Act
        var result = await _useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(barbeariaId);
        result.Name.Should().Be("Barbearia do Tasso Zé");
        result.Document.Should().Be("12345678000190");
        result.Phone.Should().Be("(11) 98765-4321");
        result.OwnerName.Should().Be("Tasso Gomes");
        result.Email.Should().Be("contato@barbeariatasso.com");
        result.Code.Should().Be("6SJJRFPD");
        result.IsActive.Should().BeTrue();
        result.Address.Street.Should().Be("Av. Paulista");
        result.Address.Number.Should().Be("1000");
        result.Address.Complement.Should().Be("Sala 15");
        result.Address.Neighborhood.Should().Be("Bela Vista");
        result.Address.City.Should().Be("São Paulo");
        result.Address.State.Should().Be("SP");
        result.Address.ZipCode.Should().Be("01310100");

        _tenantContextMock.Verify(x => x.BarbeariaId, Times.Once);
        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(barbeariaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoBarbeariaId_ShouldThrowForbiddenException()
    {
        // Arrange
        _tenantContextMock
            .Setup(x => x.BarbeariaId)
            .Returns((Guid?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _useCase.ExecuteAsync(CancellationToken.None));

        exception.Message.Should().Be("Usuário não associado a uma barbearia");

        _tenantContextMock.Verify(x => x.BarbeariaId, Times.Once);
        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_BarbeariaNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        _tenantContextMock
            .Setup(x => x.BarbeariaId)
            .Returns(barbeariaId);

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.ExecuteAsync(CancellationToken.None));

        exception.Message.Should().Be("Barbearia não encontrada");

        _tenantContextMock.Verify(x => x.BarbeariaId, Times.Once);
        _barbershopRepositoryMock.Verify(x => x.GetByIdAsync(barbeariaId, It.IsAny<CancellationToken>()), Times.Once);
    }
}