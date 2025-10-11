// BarbApp.Application.Tests/UseCases/ListBarbeirosBarbeariaUseCaseTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class ListBarbeirosBarbeariaUseCaseTests
{
    private readonly Mock<IBarberRepository> _repositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly ListBarbeirosBarbeariaUseCase _useCase;

    public ListBarbeirosBarbeariaUseCaseTests()
    {
        _repositoryMock = new Mock<IBarberRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _useCase = new ListBarbeirosBarbeariaUseCase(
            _repositoryMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task Execute_ValidContext_ShouldReturnBarberList()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbeariaCode = BarbeariaCode.Create("ABC23456");
        var barbearia = Barbershop.Create("Barbearia Teste", barbeariaCode);
        var barber1 = Barber.Create(barbearia.Id, "11987654321", "João Silva");
        var barber2 = Barber.Create(barbearia.Id, "11987654322", "Maria Santos");

        // Set navigation properties for the test
        barber1.GetType().GetProperty("Barbearia")?.SetValue(barber1, barbearia);
        barber2.GetType().GetProperty("Barbearia")?.SetValue(barber2, barbearia);

        var barbers = new List<Barber> { barber1, barber2 };

        _tenantContextMock
            .Setup(x => x.BarbeariaId)
            .Returns(barbeariaId);

        _repositoryMock
            .Setup(x => x.GetByBarbeariaIdAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbers);

        // Act
        var result = await _useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].Id.Should().Be(barber1.Id);
        resultList[0].Nome.Should().Be(barber1.Name);
        resultList[0].Telefone.Should().Be(barber1.Telefone);
        resultList[0].BarbeariaId.Should().Be(barbearia.Id);
        resultList[0].NomeBarbearia.Should().Be(barbearia.Name);

        resultList[1].Id.Should().Be(barber2.Id);
        resultList[1].Nome.Should().Be(barber2.Name);
        resultList[1].Telefone.Should().Be(barber2.Telefone);
        resultList[1].BarbeariaId.Should().Be(barbearia.Id);
        resultList[1].NomeBarbearia.Should().Be(barbearia.Name);

        _tenantContextMock.Verify(x => x.BarbeariaId, Times.Once);
        _repositoryMock.Verify(x => x.GetByBarbeariaIdAsync(barbeariaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_NoBarbeariaContext_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _tenantContextMock
            .Setup(x => x.BarbeariaId)
            .Returns((Guid?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Contexto de barbearia não definido");

        _tenantContextMock.Verify(x => x.BarbeariaId, Times.Once);
        _repositoryMock.Verify(x => x.GetByBarbeariaIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_EmptyBarberList_ShouldReturnEmptyList()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbers = new List<Barber>();

        _tenantContextMock
            .Setup(x => x.BarbeariaId)
            .Returns(barbeariaId);

        _repositoryMock
            .Setup(x => x.GetByBarbeariaIdAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbers);

        // Act
        var result = await _useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _tenantContextMock.Verify(x => x.BarbeariaId, Times.Once);
        _repositoryMock.Verify(x => x.GetByBarbeariaIdAsync(barbeariaId, It.IsAny<CancellationToken>()), Times.Once);
    }
}