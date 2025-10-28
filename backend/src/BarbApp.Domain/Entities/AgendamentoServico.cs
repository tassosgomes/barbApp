using BarbApp.Domain.Common;

namespace BarbApp.Domain.Entities;

public class AgendamentoServico
{
    public Guid AgendamentoId { get; private set; }
    public Guid ServicoId { get; private set; }

    // Navigation properties
    public Agendamento? Agendamento { get; private set; }
    public BarbershopService? Servico { get; private set; }

    private AgendamentoServico()
    {
        // EF Core constructor
    }

    public static AgendamentoServico Create(Guid agendamentoId, Guid servicoId)
    {
        return new AgendamentoServico
        {
            AgendamentoId = agendamentoId,
            ServicoId = servicoId
        };
    }
}