using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class CancelarAgendamentoUseCaseTests
{
    private readonly Mock<IAgendamentoRepository> _agendamentoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDisponibilidadeCache> _cacheMock;
    private readonly Mock<ILogger<CancelarAgendamentoUseCase>> _loggerMock;
    private readonly CancelarAgendamentoUseCase _useCase;

    public CancelarAgendamentoUseCaseTests()
    {
        _agendamentoRepositoryMock = new Mock<IAgendamentoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheMock = new Mock<IDisponibilidadeCache>();
        _loggerMock = new Mock<ILogger<CancelarAgendamentoUseCase>>();

        _useCase = new CancelarAgendamentoUseCase(
            _agendamentoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCancelAppointment()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var agendamentoId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var dataHora = DateTime.UtcNow.AddHours(2);

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        var agendamento = Agendamento.Create(
            barbeariaId, clienteId, barbeiroId, new List<Guid> { servicoId }, dataHora, 30);

        // Set the appointment ID
        typeof(Agendamento).GetProperty("Id")?.SetValue(agendamento, agendamentoId);

        _agendamentoRepositoryMock
            .Setup(x => x.GetByIdAsync(agendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendamento);

        // Act
        await _useCase.Handle(clienteId, agendamentoId, CancellationToken.None);

        // Assert
        agendamento.Status.Should().Be(StatusAgendamento.Cancelado);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.InvalidateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var agendamentoId = Guid.NewGuid();

        _agendamentoRepositoryMock
            .Setup(x => x.GetByIdAsync(agendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Agendamento?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.Handle(clienteId, agendamentoId, CancellationToken.None));

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(x => x.InvalidateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AppointmentBelongsToDifferentClient_ShouldThrowForbiddenException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var differentClienteId = Guid.NewGuid();
        var agendamentoId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var dataHora = DateTime.UtcNow.AddHours(2);

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        var agendamento = Agendamento.Create(
            barbeariaId, differentClienteId, barbeiroId, new List<Guid> { servicoId }, dataHora, 30);

        _agendamentoRepositoryMock
            .Setup(x => x.GetByIdAsync(agendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendamento);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _useCase.Handle(clienteId, agendamentoId, CancellationToken.None));

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(x => x.InvalidateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CompletedAppointment_ShouldThrowValidationException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var agendamentoId = Guid.NewGuid();
        var barbeiroId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var dataHora = DateTime.UtcNow.AddHours(2); // Future date

        var cliente = Cliente.Create(barbeariaId, "João Silva", "11987654321");
        var barbeiro = Barber.Create(barbeariaId, "Carlos Santos", "carlos@test.com", "hash123", "11987654321");
        var servico = BarbershopService.Create(barbeariaId, "Corte", "Corte masculino", 30, 25.00m);

        var agendamento = Agendamento.Create(
            barbeariaId, clienteId, barbeiroId, new List<Guid> { servicoId }, dataHora, 30);
        agendamento.Confirmar(); // First confirm the appointment
        agendamento.Concluir(); // Then mark as completed

        _agendamentoRepositoryMock
            .Setup(x => x.GetByIdAsync(agendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendamento);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidAppointmentStatusTransitionException>(
            () => _useCase.Handle(clienteId, agendamentoId, CancellationToken.None));

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(x => x.InvalidateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}