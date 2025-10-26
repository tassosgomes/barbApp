using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Entities;

public class Agendamento
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid BarbeiroId { get; private set; }
    public Guid ServicoId { get; private set; }
    public DateTime DataHora { get; private set; }
    public int DuracaoMinutos { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public DateTime? DataCancelamento { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Cliente? Cliente { get; private set; }
    public Barber? Barbeiro { get; private set; }
    public BarbershopService? Servico { get; private set; }
    public Barbershop? Barbearia { get; private set; }

    private Agendamento()
    {
        // EF Core constructor
    }

    public static Agendamento Create(
        Guid barbeariaId,
        Guid clienteId,
        Guid barbeiroId,
        Guid servicoId,
        DateTime dataHora,
        int duracaoMinutos)
    {
        var dataHoraValidada = ValidarDataHoraFutura(dataHora);
        var duracaoValidada = ValidarDuracao(duracaoMinutos);

        var now = DateTime.UtcNow;

        return new Agendamento
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            ClienteId = clienteId,
            BarbeiroId = barbeiroId,
            ServicoId = servicoId,
            DataHora = dataHoraValidada,
            DuracaoMinutos = duracaoValidada,
            Status = StatusAgendamento.Pendente,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static DateTime ValidarDataHoraFutura(DateTime dataHora)
    {
        if (dataHora <= DateTime.UtcNow)
            throw new ValidationException("Data/hora deve ser futura");

        return dataHora;
    }

    private static int ValidarDuracao(int duracaoMinutos)
    {
        if (duracaoMinutos <= 0 || duracaoMinutos > 480)
            throw new ValidationException("Duração inválida");

        return duracaoMinutos;
    }

    public void Confirmar()
    {
        if (Status != StatusAgendamento.Pendente)
            throw new InvalidAppointmentStatusTransitionException(
                Status.ToString(),
                "confirmar");

        Status = StatusAgendamento.Confirmado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status == StatusAgendamento.Concluido || Status == StatusAgendamento.Cancelado)
            throw new InvalidAppointmentStatusTransitionException(
                Status.ToString(),
                "cancelar");

        if (DataHora <= DateTime.UtcNow)
            throw new ValidationException("Não é possível cancelar agendamento passado");

        Status = StatusAgendamento.Cancelado;
        DataCancelamento = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Concluir()
    {
        if (Status != StatusAgendamento.Confirmado)
            throw new InvalidAppointmentStatusTransitionException(
                Status.ToString(),
                "concluir");

        Status = StatusAgendamento.Concluido;
        UpdatedAt = DateTime.UtcNow;
    }
}
