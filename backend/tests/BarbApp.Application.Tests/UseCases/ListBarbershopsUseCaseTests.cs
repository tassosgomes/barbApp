using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Common;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class ListBarbershopsUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly ListBarbershopsUseCase _useCase;

    public ListBarbershopsUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _useCase = new ListBarbershopsUseCase(_barbershopRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ShouldReturnPaginatedResult()
    {
        // Arrange
        var address1 = Address.Create("01310100", "Av. Paulista", "1000", "Sala 15", "Bela Vista", "São Paulo", "SP");
        var barbershop1 = Barbershop.Create(
            "Barbearia Alpha",
            Document.Create("12345678000190"),
            "11987654321",
            "João Silva",
            "joao@test.com",
            address1,
            UniqueCode.Create("ABCDEFGH"),
            "admin");

        var address2 = Address.Create("01310200", "Rua Augusta", "500", null, "Consolação", "São Paulo", "SP");
        var barbershop2 = Barbershop.Create(
            "Barbearia Beta",
            Document.Create("98765432000150"),
            "11987654322",
            "Maria Santos",
            "maria@test.com",
            address2,
            UniqueCode.Create("BCDEFGHJ"),
            "admin");

        var barbershops = new List<Barbershop> { barbershop1, barbershop2 };
        var paginatedResult = new PaginatedResult<Barbershop>(barbershops, 2, 1, 10);

        _barbershopRepositoryMock
            .Setup(x => x.ListAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _useCase.ExecuteAsync(1, 10, null, null, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        result.Items[0].Name.Should().Be("Barbearia Alpha");
        result.Items[0].Code.Should().Be("ABCDEFGH");
        result.Items[1].Name.Should().Be("Barbearia Beta");
        result.Items[1].Code.Should().Be("BCDEFGHJ");

        _barbershopRepositoryMock.Verify(x => x.ListAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSearchTerm_ShouldFilterResults()
    {
        // Arrange
        var address = Address.Create("01310100", "Av. Paulista", "1000", "Sala 15", "Bela Vista", "São Paulo", "SP");
        var barbershop = Barbershop.Create(
            "Barbearia Alpha",
            Document.Create("12345678000190"),
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            UniqueCode.Create("ABCDEFGH"),
            "admin");

        var barbershops = new List<Barbershop> { barbershop };
        var paginatedResult = new PaginatedResult<Barbershop>(barbershops, 1, 1, 10);

        _barbershopRepositoryMock
            .Setup(x => x.ListAsync(1, 10, "Alpha", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _useCase.ExecuteAsync(1, 10, "Alpha", null, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Barbearia Alpha");

        _barbershopRepositoryMock.Verify(x => x.ListAsync(1, 10, "Alpha", null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithIsActiveFilter_ShouldFilterResults()
    {
        // Arrange
        var address = Address.Create("01310100", "Av. Paulista", "1000", "Sala 15", "Bela Vista", "São Paulo", "SP");
        var barbershop = Barbershop.Create(
            "Barbearia Alpha",
            Document.Create("12345678000190"),
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            UniqueCode.Create("ABCDEFGH"),
            "admin");

        var barbershops = new List<Barbershop> { barbershop };
        var paginatedResult = new PaginatedResult<Barbershop>(barbershops, 1, 1, 10);

        _barbershopRepositoryMock
            .Setup(x => x.ListAsync(1, 10, null, true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _useCase.ExecuteAsync(1, 10, null, true, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        _barbershopRepositoryMock.Verify(x => x.ListAsync(1, 10, null, true, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSortBy_ShouldSortResults()
    {
        // Arrange
        var barbershops = new List<Barbershop>();
        var paginatedResult = new PaginatedResult<Barbershop>(barbershops, 0, 1, 10);

        _barbershopRepositoryMock
            .Setup(x => x.ListAsync(1, 10, null, null, "name", It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _useCase.ExecuteAsync(1, 10, null, null, "name", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();

        _barbershopRepositoryMock.Verify(x => x.ListAsync(1, 10, null, null, "name", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var barbershops = new List<Barbershop>();
        var paginatedResult = new PaginatedResult<Barbershop>(barbershops, 0, 1, 10);

        _barbershopRepositoryMock
            .Setup(x => x.ListAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _useCase.ExecuteAsync(1, 10, null, null, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        _barbershopRepositoryMock.Verify(x => x.ListAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}