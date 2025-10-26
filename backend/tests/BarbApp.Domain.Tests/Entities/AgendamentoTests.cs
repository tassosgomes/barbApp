using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class AgendamentoTests
{
    private readonly Guid _barbeariaId = Guid.NewGuid();
    private readonly Guid _clienteId = Guid.NewGuid();
    private readonly Guid _barbeiroId = Guid.NewGuid();
    private readonly Guid _servicoId = Guid.NewGuid();
    private readonly DateTime _dataHoraFutura = DateTime.UtcNow.AddHours(2);

    [Fact]
    public void Create_ComDadosValidos_DeveCriarAgendamentoComStatusPendente()
    {
        // Arrange
        var duracaoMinutos = 30;

        // Act
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            duracaoMinutos);

        // Assert
        agendamento.Should().NotBeNull();
        agendamento.Id.Should().NotBe(Guid.Empty);
        agendamento.BarbeariaId.Should().Be(_barbeariaId);
        agendamento.ClienteId.Should().Be(_clienteId);
        agendamento.BarbeiroId.Should().Be(_barbeiroId);
        agendamento.ServicoId.Should().Be(_servicoId);
        agendamento.DataHora.Should().Be(_dataHoraFutura);
        agendamento.DuracaoMinutos.Should().Be(duracaoMinutos);
        agendamento.Status.Should().Be(StatusAgendamento.Pendente);
        agendamento.DataCancelamento.Should().BeNull();
        agendamento.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        agendamento.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ComDataPassada_DeveLancarValidationException()
    {
        // Arrange
        var dataPassada = DateTime.UtcNow.AddHours(-1);
        var duracaoMinutos = 30;

        // Act
        var act = () => Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            dataPassada,
            duracaoMinutos);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Data/hora deve ser futura");
    }

    [Fact]
    public void Create_ComDataAtual_DeveLancarValidationException()
    {
        // Arrange
        var dataAtual = DateTime.UtcNow;
        var duracaoMinutos = 30;

        // Act
        var act = () => Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            dataAtual,
            duracaoMinutos);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Data/hora deve ser futura");
    }

    [Theory]
    [InlineData(1)] // Mínimo válido
    [InlineData(30)] // Típico
    [InlineData(60)] // Uma hora
    [InlineData(480)] // Máximo válido (8 horas)
    public void Create_ComDuracaoValida_DeveCriarComSucesso(int duracaoMinutos)
    {
        // Act
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            duracaoMinutos);

        // Assert
        agendamento.DuracaoMinutos.Should().Be(duracaoMinutos);
    }

    [Theory]
    [InlineData(0)] // Zero
    [InlineData(-1)] // Negativo
    [InlineData(481)] // Acima do máximo
    [InlineData(1000)] // Muito grande
    public void Create_ComDuracaoInvalida_DeveLancarValidationException(int duracaoMinutos)
    {
        // Act
        var act = () => Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            duracaoMinutos);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Duração inválida");
    }

    [Fact]
    public void Confirmar_AgendamentoPendente_DeveAtualizarStatusParaConfirmado()
    {
        // Arrange
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            30);
        var updateAtOriginal = agendamento.UpdatedAt;

        // Act
        Thread.Sleep(10); // Garantir diferença de tempo
        agendamento.Confirmar();

        // Assert
        agendamento.Status.Should().Be(StatusAgendamento.Confirmado);
        agendamento.UpdatedAt.Should().BeAfter(updateAtOriginal);
    }

    [Theory]
    [InlineData(StatusAgendamento.Confirmado)]
    [InlineData(StatusAgendamento.Concluido)]
    [InlineData(StatusAgendamento.Cancelado)]
    public void Confirmar_AgendamentoNaoPendente_DeveLancarInvalidAppointmentStatusTransitionException(
        StatusAgendamento status)
    {
        // Arrange
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            30);
        SetAgendamentoStatus(agendamento, status);

        // Act
        var act = () => agendamento.Confirmar();

        // Assert
        act.Should().Throw<InvalidAppointmentStatusTransitionException>()
            .WithMessage($"Cannot confirmar appointment with status '{status}'");
    }

    [Theory]
    [InlineData(StatusAgendamento.Pendente)]
    [InlineData(StatusAgendamento.Confirmado)]
    public void Cancelar_AgendamentoPendenteOuConfirmado_DeveAtualizarStatusParaCancelado(
        StatusAgendamento statusInicial)
    {
        // Arrange
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            30);
        SetAgendamentoStatus(agendamento, statusInicial);
        var updateAtOriginal = agendamento.UpdatedAt;

        // Act
        Thread.Sleep(10); // Garantir diferença de tempo
        agendamento.Cancelar();

        // Assert
        agendamento.Status.Should().Be(StatusAgendamento.Cancelado);
        agendamento.DataCancelamento.Should().NotBeNull();
        agendamento.DataCancelamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        agendamento.UpdatedAt.Should().BeAfter(updateAtOriginal);
    }

    [Theory]
    [InlineData(StatusAgendamento.Concluido)]
    [InlineData(StatusAgendamento.Cancelado)]
    public void Cancelar_AgendamentoConcluidoOuCancelado_DeveLancarInvalidAppointmentStatusTransitionException(
        StatusAgendamento status)
    {
        // Arrange
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            30);
        SetAgendamentoStatus(agendamento, status);

        // Act
        var act = () => agendamento.Cancelar();

        // Assert
        act.Should().Throw<InvalidAppointmentStatusTransitionException>()
            .WithMessage($"Cannot cancelar appointment with status '{status}'");
    }

    [Fact]
    public void Cancelar_AgendamentoPassado_DeveLancarValidationException()
    {
        // Arrange
        var dataPassada = DateTime.UtcNow.AddHours(-1);
        var agendamento = CriarAgendamentoComDataEspecifica(dataPassada);

        // Act
        var act = () => agendamento.Cancelar();

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Não é possível cancelar agendamento passado");
    }

    [Fact]
    public void Concluir_AgendamentoConfirmado_DeveAtualizarStatusParaConcluido()
    {
        // Arrange
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            30);
        agendamento.Confirmar();
        var updateAtOriginal = agendamento.UpdatedAt;

        // Act
        Thread.Sleep(10); // Garantir diferença de tempo
        agendamento.Concluir();

        // Assert
        agendamento.Status.Should().Be(StatusAgendamento.Concluido);
        agendamento.UpdatedAt.Should().BeAfter(updateAtOriginal);
    }

    [Theory]
    [InlineData(StatusAgendamento.Pendente)]
    [InlineData(StatusAgendamento.Concluido)]
    [InlineData(StatusAgendamento.Cancelado)]
    public void Concluir_AgendamentoNaoConfirmado_DeveLancarInvalidAppointmentStatusTransitionException(
        StatusAgendamento status)
    {
        // Arrange
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            _dataHoraFutura,
            30);
        SetAgendamentoStatus(agendamento, status);

        // Act
        var act = () => agendamento.Concluir();

        // Assert
        act.Should().Throw<InvalidAppointmentStatusTransitionException>()
            .WithMessage($"Cannot concluir appointment with status '{status}'");
    }

    // Helper method to set status using reflection
    private void SetAgendamentoStatus(Agendamento agendamento, StatusAgendamento status)
    {
        var statusProperty = typeof(Agendamento).GetProperty("Status");
        statusProperty!.SetValue(agendamento, status);

        // Update UpdatedAt as well
        var updatedAtProperty = typeof(Agendamento).GetProperty("UpdatedAt");
        updatedAtProperty!.SetValue(agendamento, DateTime.UtcNow);
    }

    // Helper method to create agendamento with specific data using reflection
    private Agendamento CriarAgendamentoComDataEspecifica(DateTime dataHora)
    {
        // Create with future date first
        var agendamento = Agendamento.Create(
            _barbeariaId,
            _clienteId,
            _barbeiroId,
            _servicoId,
            DateTime.UtcNow.AddHours(2),
            30);

        // Then set the past date using reflection
        var dataHoraProperty = typeof(Agendamento).GetProperty("DataHora");
        dataHoraProperty!.SetValue(agendamento, dataHora);

        return agendamento;
    }
}
