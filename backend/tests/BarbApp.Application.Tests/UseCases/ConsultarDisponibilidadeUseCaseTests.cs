// BarbApp.Application.Tests/UseCases/ConsultarDisponibilidadeUseCaseTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class ConsultarDisponibilidadeUseCaseTests
{
    private readonly Mock<IBarberRepository> _barbeirosRepositoryMock;
    private readonly Mock<IAgendamentoRepository> _agendamentoRepositoryMock;
    private readonly Mock<IDisponibilidadeCache> _cacheMock;
    private readonly Mock<ILogger<ConsultarDisponibilidadeUseCase>> _loggerMock;
    private readonly ConsultarDisponibilidadeUseCase _useCase;

    public ConsultarDisponibilidadeUseCaseTests()
    {
        _barbeirosRepositoryMock = new Mock<IBarberRepository>();
        _agendamentoRepositoryMock = new Mock<IAgendamentoRepository>();
        _cacheMock = new Mock<IDisponibilidadeCache>();
        _loggerMock = new Mock<ILogger<ConsultarDisponibilidadeUseCase>>();
        _useCase = new ConsultarDisponibilidadeUseCase(
            _barbeirosRepositoryMock.Object,
            _agendamentoRepositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ComCacheHit_DeveRetornarDoCacheSemConsultarRepositorios()
    {
        // Arrange
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(6);
        var duracaoMinutos = 30;

        var cachedResult = new DisponibilidadeOutput(
            new BarbeiroDto(barbeiroId, "João Silva", null, new List<string>()),
            new List<DiaDisponivel>
            {
                new DiaDisponivel(dataInicio, new List<string> { "09:00", "09:30" })
            });

        _cacheMock
            .Setup(x => x.GetAsync(barbeiroId, dataInicio, dataFim, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);

        // Act
        var result = await _useCase.Handle(barbeiroId, dataInicio, dataFim, duracaoMinutos);

        // Assert
        result.Should().Be(cachedResult);
        _barbeirosRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _agendamentoRepositoryMock.Verify(x => x.GetByBarbeiroAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(x => x.SetAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DisponibilidadeOutput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComCacheMiss_DeveCalcularDisponibilidadeESalvarNoCache()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var dataInicio = new DateTime(2025, 10, 15); // Quarta-feira
        var dataFim = dataInicio.AddDays(2);
        var duracaoMinutos = 30;

        var barbeiro = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321");
        typeof(Barber).GetProperty("Id")!.SetValue(barbeiro, barbeiroId);

        _cacheMock
            .Setup(x => x.GetAsync(barbeiroId, dataInicio, dataFim, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisponibilidadeOutput?)null);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _agendamentoRepositoryMock
            .Setup(x => x.GetByBarbeiroAndDateRangeAsync(barbeiroId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agendamento>());

        // Act
        var result = await _useCase.Handle(barbeiroId, dataInicio, dataFim, duracaoMinutos);

        // Assert
        result.Should().NotBeNull();
        result.Barbeiro.Id.Should().Be(barbeiroId);
        result.Barbeiro.Nome.Should().Be("João Silva");
        result.DiasDisponiveis.Should().HaveCount(3); // 3 dias

        // Verificar que todos os dias têm slots disponíveis (sem agendamentos)
        foreach (var dia in result.DiasDisponiveis)
        {
            dia.HorariosDisponiveis.Should().NotBeEmpty();
            // Slots de 08:00 às 20:00 = 24 slots (48/2)
            dia.HorariosDisponiveis.Should().HaveCount(24);
        }

        _cacheMock.Verify(x => x.SetAsync(barbeiroId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DisponibilidadeOutput>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComAgendamentoDe30Min_DeveBloquearApenasUmSlot()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.AddDays(1).Date; // Amanhã
        var dataFim = dataInicio;
        var duracaoMinutos = 30;

        var barbeiro = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321");
        typeof(Barber).GetProperty("Id")!.SetValue(barbeiro, barbeiroId);

        // Agendamento de 30min às 10:00
        var agendamento = Agendamento.Create(
            barbeariaId,
            Guid.NewGuid(),
            barbeiroId,
            new List<Guid> { Guid.NewGuid() },
            dataInicio.AddHours(10), // Mesmo dia às 10:00
            30);

        _cacheMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisponibilidadeOutput?)null);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _agendamentoRepositoryMock
            .Setup(x => x.GetByBarbeiroAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agendamento> { agendamento });

        // Act
        var result = await _useCase.Handle(barbeiroId, dataInicio, dataFim, duracaoMinutos);

        // Assert
        var dia = result.DiasDisponiveis[0];
        dia.HorariosDisponiveis.Should().NotContain("10:00");
        dia.HorariosDisponiveis.Should().Contain("09:30");
        dia.HorariosDisponiveis.Should().Contain("10:30");
        dia.HorariosDisponiveis.Should().HaveCount(23); // 24 - 1 bloqueado
    }

    [Fact]
    public async Task Handle_ComAgendamentoDe60Min_DeveBloquearDoisSlots()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.AddDays(1).Date; // Amanhã
        var dataFim = dataInicio;
        var duracaoMinutos = 30;

        var barbeiro = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321");
        typeof(Barber).GetProperty("Id")!.SetValue(barbeiro, barbeiroId);

        // Agendamento de 60min às 10:00 (bloqueia 10:00 e 10:30)
        var agendamento = Agendamento.Create(
            barbeariaId,
            Guid.NewGuid(),
            barbeiroId,
            new List<Guid> { Guid.NewGuid() },
            dataInicio.AddHours(10), // Mesmo dia às 10:00
            60);

        _cacheMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisponibilidadeOutput?)null);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _agendamentoRepositoryMock
            .Setup(x => x.GetByBarbeiroAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agendamento> { agendamento });

        // Act
        var result = await _useCase.Handle(barbeiroId, dataInicio, dataFim, duracaoMinutos);

        // Assert
        var dia = result.DiasDisponiveis[0];
        dia.HorariosDisponiveis.Should().NotContain("10:00");
        dia.HorariosDisponiveis.Should().NotContain("10:30");
        dia.HorariosDisponiveis.Should().Contain("09:30");
        dia.HorariosDisponiveis.Should().Contain("11:00");
        dia.HorariosDisponiveis.Should().HaveCount(22); // 24 - 2 bloqueados
    }

    [Fact]
    public async Task Handle_AgendamentoCancelado_NaoDeveBloquearHorario()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.AddDays(1).Date; // Amanhã
        var dataFim = dataInicio;
        var duracaoMinutos = 30;

        var barbeiro = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321");
        typeof(Barber).GetProperty("Id")!.SetValue(barbeiro, barbeiroId);

        // Agendamento cancelado
        var agendamento = Agendamento.Create(
            barbeariaId,
            Guid.NewGuid(),
            barbeiroId,
            new List<Guid> { Guid.NewGuid() },
            dataInicio.AddHours(10), // Mesmo dia às 10:00
            30);
        agendamento.Cancelar(); // Status = Cancelado

        _cacheMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisponibilidadeOutput?)null);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _agendamentoRepositoryMock
            .Setup(x => x.GetByBarbeiroAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agendamento> { agendamento });

        // Act
        var result = await _useCase.Handle(barbeiroId, dataInicio, dataFim, duracaoMinutos);

        // Assert
        var dia = result.DiasDisponiveis[0];
        dia.HorariosDisponiveis.Should().Contain("10:00"); // Não deve estar bloqueado
        dia.HorariosDisponiveis.Should().HaveCount(24); // Todos disponíveis
    }

    [Fact]
    public async Task Handle_DataHoje_DeveRemoverHorariosPassados()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date.AddDays(1); // Amanhã
        var dataFim = dataInicio;
        var duracaoMinutos = 30;

        var barbeiro = Barber.Create(barbeariaId, "João Silva", "joao@test.com", "hash", "11987654321");
        typeof(Barber).GetProperty("Id")!.SetValue(barbeiro, barbeiroId);

        _cacheMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisponibilidadeOutput?)null);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);

        _agendamentoRepositoryMock
            .Setup(x => x.GetByBarbeiroAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agendamento>());

        // Act
        var result = await _useCase.Handle(barbeiroId, dataInicio, dataFim, duracaoMinutos);

        // Assert
        var dia = result.DiasDisponiveis[0];
        // Para amanhã, todos os horários devem estar disponíveis (nenhum passado)
        dia.HorariosDisponiveis.Should().HaveCount(24); // Slots de 08:00 às 20:00
        // Verificar que o primeiro horário é 08:00
        dia.HorariosDisponiveis.First().Should().Be("08:00");
    }

    [Fact]
    public async Task Handle_BarbeiroInativo_DeveLancarNotFoundException()
    {
        // Arrange
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(6);
        var duracaoMinutos = 30;

        _cacheMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisponibilidadeOutput?)null);

        _barbeirosRepositoryMock
            .Setup(x => x.GetByIdAsync(barbeiroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.Handle(barbeiroId, dataInicio, dataFim, duracaoMinutos));
    }
}