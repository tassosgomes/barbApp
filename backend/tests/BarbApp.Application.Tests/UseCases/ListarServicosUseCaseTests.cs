// BarbApp.Application.Tests/UseCases/ListarServicosUseCaseTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class ListarServicosUseCaseTests
{
    private readonly Mock<IBarbershopServiceRepository> _repositoryMock;
    private readonly Mock<ILogger<ListarServicosUseCase>> _loggerMock;
    private readonly ListarServicosUseCase _useCase;

    public ListarServicosUseCaseTests()
    {
        _repositoryMock = new Mock<IBarbershopServiceRepository>();
        _loggerMock = new Mock<ILogger<ListarServicosUseCase>>();
        _useCase = new ListarServicosUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarApenasServicosAtivos()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var service1 = BarbershopService.Create(barbeariaId, "Corte", "Corte de cabelo", 30, 50.0m);
        var service2 = BarbershopService.Create(barbeariaId, "Barba", "Aparar barba", 20, 30.0m);
        var services = new List<BarbershopService> { service1, service2 };

        _repositoryMock
            .Setup(x => x.ListAsync(barbeariaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _useCase.Handle(barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result[0].Id.Should().Be(service1.Id);
        result[0].Nome.Should().Be(service1.Name);
        result[0].Descricao.Should().Be(service1.Description);
        result[0].DuracaoMinutos.Should().Be(service1.DurationMinutes);
        result[0].Preco.Should().Be(service1.Price);

        result[1].Id.Should().Be(service2.Id);
        result[1].Nome.Should().Be(service2.Name);
        result[1].Descricao.Should().Be(service2.Description);
        result[1].DuracaoMinutos.Should().Be(service2.DurationMinutes);
        result[1].Preco.Should().Be(service2.Price);

        _repositoryMock.Verify(x => x.ListAsync(barbeariaId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SemServicos_DeveRetornarListaVazia()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var services = new List<BarbershopService>();

        _repositoryMock
            .Setup(x => x.ListAsync(barbeariaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _useCase.Handle(barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _repositoryMock.Verify(x => x.ListAsync(barbeariaId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ServicoSemDescricao_DeveRetornarDescricaoVazia()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var service = BarbershopService.Create(barbeariaId, "Corte", null, 30, 50.0m);
        var services = new List<BarbershopService> { service };

        _repositoryMock
            .Setup(x => x.ListAsync(barbeariaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _useCase.Handle(barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Descricao.Should().BeEmpty();
    }
}