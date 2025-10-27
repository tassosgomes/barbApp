// BarbApp.Application.Tests/UseCases/ListarBarbeirosUseCaseTests.cs
using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class ListarBarbeirosUseCaseTests
{
    private readonly Mock<IBarbeirosRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ListarBarbeirosUseCase>> _loggerMock;
    private readonly ListarBarbeirosUseCase _useCase;

    public ListarBarbeirosUseCaseTests()
    {
        _repositoryMock = new Mock<IBarbeirosRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ListarBarbeirosUseCase>>();
        _useCase = new ListarBarbeirosUseCase(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarApenasBarbeirosAtivos()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barber1 = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hashedpassword1", "11987654321");
        var barber2 = Barber.Create(barbeariaId, "Maria Santos", "maria@test.com", "hashedpassword2", "11987654322");
        var barbers = new List<Barber> { barber1, barber2 };
        var expectedDtos = new List<BarbeiroDto>
        {
            new BarbeiroDto(barber1.Id, barber1.Name, null, new List<string>()),
            new BarbeiroDto(barber2.Id, barber2.Name, null, new List<string>())
        };

        _repositoryMock
            .Setup(x => x.GetAtivosAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbers);

        _mapperMock
            .Setup(x => x.Map<List<BarbeiroDto>>(barbers))
            .Returns(expectedDtos);

        // Act
        var result = await _useCase.Handle(barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result[0].Id.Should().Be(barber1.Id);
        result[0].Nome.Should().Be(barber1.Name);
        result[0].Foto.Should().BeNull();
        result[0].Especialidades.Should().BeEmpty();

        result[1].Id.Should().Be(barber2.Id);
        result[1].Nome.Should().Be(barber2.Name);
        result[1].Foto.Should().BeNull();
        result[1].Especialidades.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAtivosAsync(barbeariaId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<List<BarbeiroDto>>(barbers), Times.Once);
    }

    [Fact]
    public async Task Handle_SemBarbeiros_DeveRetornarListaVazia()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbers = new List<Barber>();
        var expectedDtos = new List<BarbeiroDto>();

        _repositoryMock
            .Setup(x => x.GetAtivosAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbers);

        _mapperMock
            .Setup(x => x.Map<List<BarbeiroDto>>(barbers))
            .Returns(expectedDtos);

        // Act
        var result = await _useCase.Handle(barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAtivosAsync(barbeariaId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<List<BarbeiroDto>>(barbers), Times.Once);
    }
}