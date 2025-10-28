// BarbApp.Application/UseCases/ListarAgendamentosClienteUseCase.cs
using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListarAgendamentosClienteUseCase : IListarAgendamentosClienteUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListarAgendamentosClienteUseCase> _logger;

    public ListarAgendamentosClienteUseCase(
        IAgendamentoRepository agendamentoRepository,
        IMapper mapper,
        ILogger<ListarAgendamentosClienteUseCase> logger)
    {
        _agendamentoRepository = agendamentoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<AgendamentoOutput>> Handle(
        Guid clienteId,
        string filtro,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Listando agendamentos do cliente {ClienteId} com filtro '{Filtro}'",
            clienteId, filtro);

        StatusAgendamento? statusFiltro = filtro.ToLower() switch
        {
            "proximos" => null, // Retorna Pendente e Confirmado
            "historico" => null, // Retorna Concluído e Cancelado
            _ => null
        };

        var agendamentos = await _agendamentoRepository.GetByClienteAsync(
            clienteId,
            statusFiltro,
            cancellationToken);

        // Filtrar por data e status
        IEnumerable<Agendamento> agendamentosFiltrados;
        if (filtro.ToLower() == "proximos")
        {
            agendamentosFiltrados = agendamentos
                .Where(a => a.DataHora >= DateTime.UtcNow &&
                           (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado))
                .OrderBy(a => a.DataHora);
        }
        else if (filtro.ToLower() == "historico")
        {
            agendamentosFiltrados = agendamentos
                .Where(a => a.Status == StatusAgendamento.Concluido || a.Status == StatusAgendamento.Cancelado)
                .OrderByDescending(a => a.DataHora);
        }
        else
        {
            // Filtro inválido, retornar todos
            agendamentosFiltrados = agendamentos.OrderByDescending(a => a.DataHora);
        }

        var result = agendamentosFiltrados.ToList();

        _logger.LogInformation(
            "Encontrados {Count} agendamentos para o cliente {ClienteId} com filtro '{Filtro}'",
            result.Count, clienteId, filtro);

        // Mapear para output (precisa incluir as entidades relacionadas)
        // Como o repositório pode não incluir as navegações, vamos mapear o que temos
        return result.Select(a => new AgendamentoOutput(
            a.Id,
            a.Barbeiro != null ? _mapper.Map<BarbeiroDto>(a.Barbeiro) : null!,
            a.AgendamentoServicos
                .Select(ag => ag.Servico)
                .Where(s => s != null)
                .Select(s => _mapper.Map<ServicoDto>(s!))
                .ToList(),
            a.DataHora,
            a.DuracaoMinutos,
            a.Status.ToString()
        )).ToList();
    }
}