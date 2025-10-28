using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Entities;

public class Agendamento
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid BarbeiroId { get; private set; }
    public DateTime DataHora { get; private set; }
    public int DuracaoMinutos { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public DateTime? DataCancelamento { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Cliente? Cliente { get; private set; }
    public Barber? Barbeiro { get; private set; }
    public Barbershop? Barbearia { get; private set; }
    public ICollection<AgendamentoServico> AgendamentoServicos { get; private set; } = new List<AgendamentoServico>();

    private Agendamento()
    {
        // EF Core constructor
    }

    public static Agendamento Create(
        Guid barbeariaId,
        Guid clienteId,
        Guid barbeiroId,
        List<Guid> servicosIds,
        DateTime dataHora,
        int duracaoMinutos)
    {
        var dataHoraValidada = ValidarDataHoraFutura(dataHora);
        var duracaoValidada = ValidarDuracao(duracaoMinutos);

        if (servicosIds == null || !servicosIds.Any())
            throw new ValidationException("Pelo menos um serviço deve ser selecionado");

        var now = DateTime.UtcNow;

        var agendamento = new Agendamento
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            ClienteId = clienteId,
            BarbeiroId = barbeiroId,
            DataHora = dataHoraValidada,
            DuracaoMinutos = duracaoValidada,
            Status = StatusAgendamento.Pendente,
            CreatedAt = now,
            UpdatedAt = now,
            AgendamentoServicos = servicosIds.Select(servicoId =>
                AgendamentoServico.Create(Guid.NewGuid(), servicoId)).ToList()
        };

        return agendamento;
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

    public void Atualizar(Guid? barbeiroId, List<Guid>? servicosIds, DateTime? dataHora, int? duracaoMinutos)
    {
        if (Status == StatusAgendamento.Concluido || Status == StatusAgendamento.Cancelado)
            throw new InvalidAppointmentStatusTransitionException(
                Status.ToString(),
                "editar");

        if (DataHora <= DateTime.UtcNow)
            throw new ValidationException("Não é possível editar agendamento passado");

        if (barbeiroId.HasValue)
            BarbeiroId = barbeiroId.Value;

        if (servicosIds != null && servicosIds.Any())
        {
            // Clear existing services and add new ones
            AgendamentoServicos.Clear();
            foreach (var servicoId in servicosIds)
            {
                AgendamentoServicos.Add(AgendamentoServico.Create(Id, servicoId));
            }
        }

        if (dataHora.HasValue)
            DataHora = ValidarDataHoraFutura(dataHora.Value);

        if (duracaoMinutos.HasValue)
            DuracaoMinutos = ValidarDuracao(duracaoMinutos.Value);

        UpdatedAt = DateTime.UtcNow;
    }
}
