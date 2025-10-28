using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class CriarAgendamentoUseCaseTests
{
    private readonly Mock<IAgendamentoRepository> _agendamentoRepositoryMock;
    private readonly Mock<IBarbeirosRepository> _barbeirosRepositoryMock;
    private readonly Mock<IServicosRepository> _servicosRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IDisponibilidadeCache> _cacheMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CriarAgendamentoUseCase>> _loggerMock;
    private readonly CriarAgendamentoUseCase _useCase;

    public CriarAgendamentoUseCaseTests()
    {
        _agendamentoRepositoryMock = new Mock<IAgendamentoRepository>();
        _barbeirosRepositoryMock = new Mock<IBarbeirosRepository>();
        _servicosRepositoryMock = new Mock<IServicosRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _cacheMock = new Mock<IDisponibilidadeCache>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CriarAgendamentoUseCase>>();

        _useCase = new CriarAgendamentoUseCase(
            _agendamentoRepositoryMock.Object,
            _barbeirosRepositoryMock.Object,
            _servicosRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _cacheMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidInput_ShouldCreateAppointmentAndReturnOutput()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var dataHora = DateTime.UtcNow.AddHours(2);

        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: dataHora
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        var agendamentoOutput = new AgendamentoOutput(
            Guid.NewGuid(),
            new BarbeiroDto(barbeiroId, "Carlos Santos", null, new List<string>()),
            new List<ServicoDto> { new ServicoDto(servicoId, "Corte", "Corte masculino", 30, 25.00m) },
            dataHora,
            30,
            "Agendado"
        );

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _servicosRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService> { servico });

        _agendamentoRepositoryMock
            .Setup(x => x.ExisteConflito(barbeiroId, dataHora, 30, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(x => x.Map<BarbeiroDto>(barbeiro))
            .Returns(new BarbeiroDto(barbeiroId, "Carlos Santos", null, new List<string>()));

        _mapperMock
            .Setup(x => x.Map<List<ServicoDto>>(It.IsAny<List<BarbershopService>>()))
            .Returns(new List<ServicoDto> { new ServicoDto(servicoId, "Corte", "Corte masculino", 30, 25.00m) });

        // Act
        AgendamentoOutput result;
        try
        {
            result = await _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None);
        }
        catch (Exception ex)
        {
            throw new Exception($"Test failed with exception: {ex.Message}", ex);
        }

        // Assert
        result.Should().NotBeNull();
        result.Barbeiro.Id.Should().Be(barbeiroId);
        result.Servicos.Should().HaveCount(1);
        result.Servicos.First().Id.Should().Be(servicoId);
        result.DataHora.Should().Be(dataHora);
        result.DuracaoTotal.Should().Be(30);
        result.Status.Should().Be("Pendente");

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _barbeirosRepositoryMock.Verify(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()), Times.Once);
        _servicosRepositoryMock.Verify(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        _agendamentoRepositoryMock.Verify(x => x.ExisteConflito(barbeiroId, dataHora, 30, null, It.IsAny<CancellationToken>()), Times.Once);
        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: DateTime.UtcNow.AddHours(2)
        );

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BarbeiroNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: DateTime.UtcNow.AddHours(2)
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BarbeiroInactive_ShouldThrowNotFoundException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: DateTime.UtcNow.AddHours(2)
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        barbeiro.Deactivate(); // Make barber inactive

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ServicoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: DateTime.UtcNow.AddHours(2)
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _servicosRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService>()); // Empty list - service not found

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BarbeiroFromDifferentBarbearia_ShouldThrowForbiddenException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var differentBarbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { Guid.NewGuid() },
            DataHora: DateTime.UtcNow.AddHours(2)
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(differentBarbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ServicoFromDifferentBarbearia_ShouldThrowForbiddenException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var differentBarbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: DateTime.UtcNow.AddHours(2)
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(differentBarbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _servicosRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService> { servico }); // Service from different barbearia

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PastDateTime_ShouldThrowValidationException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: DateTime.UtcNow.AddMinutes(-30) // Past time
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _servicosRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService> { servico });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TimeConflict_ShouldThrowHorarioIndisponivelException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var dataHora = DateTime.UtcNow.AddHours(2);

        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: dataHora
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _servicosRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService> { servico });

        _agendamentoRepositoryMock
            .Setup(x => x.ExisteConflito(barbeiroId, dataHora, 30, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Conflict exists

        // Act & Assert
        await Assert.ThrowsAsync<HorarioIndisponivelException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AppointmentAfterClosingTime_ShouldThrowValidationException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var dataHora = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 19, 45, 0); // 7:45 PM

        var input = new CriarAgendamentoInput(
            BarbeiroId: barbeiroId,
            ServicosIds: new List<Guid> { servicoId },
            DataHora: dataHora
        );

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _servicosRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService> { servico });

        _agendamentoRepositoryMock
            .Setup(x => x.ExisteConflito(barbeiroId, dataHora, 30, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.Handle(clienteId, barbeariaId, input, CancellationToken.None));

        _agendamentoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}