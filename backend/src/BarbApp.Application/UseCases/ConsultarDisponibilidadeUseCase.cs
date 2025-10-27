using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ConsultarDisponibilidadeUseCase : IConsultarDisponibilidadeUseCase
{
    private readonly IBarberRepository _barbeirosRepository;
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IDisponibilidadeCache _cache;
    private readonly ILogger<ConsultarDisponibilidadeUseCase> _logger;

    private const int SLOT_MINUTOS = 30;
    private const int HORA_INICIO = 8;
    private const int HORA_FIM = 20;

    public ConsultarDisponibilidadeUseCase(
        IBarberRepository barbeirosRepository,
        IAgendamentoRepository agendamentoRepository,
        IDisponibilidadeCache cache,
        ILogger<ConsultarDisponibilidadeUseCase> logger)
    {
        _barbeirosRepository = barbeirosRepository;
        _agendamentoRepository = agendamentoRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DisponibilidadeOutput> Handle(
        Guid barbeiroId,
        DateTime dataInicio,
        DateTime dataFim,
        int duracaoServicosMinutos,
        CancellationToken cancellationToken = default)
    {
        // 1. Tentar buscar do cache
        var cached = await _cache.GetAsync(barbeiroId, dataInicio, dataFim, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para disponibilidade do barbeiro {BarbeiroId}", barbeiroId);
            return cached;
        }

        _logger.LogDebug("Cache miss para disponibilidade do barbeiro {BarbeiroId}", barbeiroId);

        // 2. Buscar barbeiro
        var barbeiro = await _barbeirosRepository.GetByIdAsync(barbeiroId, cancellationToken);
        if (barbeiro == null || !barbeiro.IsActive)
        {
            throw new NotFoundException($"Barbeiro {barbeiroId} não encontrado ou inativo");
        }

        // 3. Buscar agendamentos existentes
        var agendamentos = await _agendamentoRepository.GetByBarbeiroAndDateRangeAsync(
            barbeiroId,
            dataInicio,
            dataFim.AddDays(1), // Incluir dia inteiro
            cancellationToken);

        // Filtrar apenas Pendente e Confirmado
        var agendamentosAtivos = agendamentos
            .Where(a => a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado)
            .ToList();

        // 4. Calcular disponibilidade
        var diasDisponiveis = new List<DiaDisponivel>();

        for (var data = dataInicio.Date; data <= dataFim.Date; data = data.AddDays(1))
        {
            // Gerar todos os slots do dia
            var todosSlots = GerarSlotsDisponiveis(data);

            // Remover slots ocupados
            var slotsDisponiveis = RemoverSlotsOcupados(todosSlots, agendamentosAtivos, duracaoServicosMinutos);

            // Remover horários passados se data for hoje
            if (data.Date == DateTime.UtcNow.Date)
            {
                slotsDisponiveis = slotsDisponiveis
                    .Where(s => s > DateTime.UtcNow)
                    .ToList();
            }

            if (slotsDisponiveis.Any())
            {
                diasDisponiveis.Add(new DiaDisponivel(
                    data,
                    slotsDisponiveis.Select(s => s.ToString("HH:mm")).ToList()
                ));
            }
        }

        // 5. Criar output
        var output = new DisponibilidadeOutput(
            new BarbeiroDto(barbeiro.Id, barbeiro.Name, null, new List<string>()),
            diasDisponiveis
        );

        // 6. Salvar no cache
        await _cache.SetAsync(barbeiroId, dataInicio, dataFim, output, cancellationToken);

        return output;
    }

    private List<DateTime> GerarSlotsDisponiveis(DateTime data)
    {
        var slots = new List<DateTime>();
        var dataBase = data.Date;

        for (int hora = HORA_INICIO; hora < HORA_FIM; hora++)
        {
            for (int minuto = 0; minuto < 60; minuto += SLOT_MINUTOS)
            {
                slots.Add(dataBase.AddHours(hora).AddMinutes(minuto));
            }
        }

        return slots;
    }

    private List<DateTime> RemoverSlotsOcupados(
        List<DateTime> slots,
        List<Agendamento> agendamentos,
        int duracaoServicosMinutos)
    {
        var slotsDisponiveis = new List<DateTime>();

        foreach (var slot in slots)
        {
            var slotTermino = slot.AddMinutes(duracaoServicosMinutos);

            // Verificar se slot conflita com algum agendamento existente
            var temConflito = agendamentos.Any(a =>
            {
                var agendamentoInicio = a.DataHora;
                var agendamentoTermino = a.DataHora.AddMinutes(a.DuracaoMinutos);

                // Lógica de sobreposição:
                // Conflita SE: (slotInicio < agendamentoTermino) E (slotTermino > agendamentoInicio)
                return (slot < agendamentoTermino) && (slotTermino > agendamentoInicio);
            });

            if (!temConflito)
            {
                slotsDisponiveis.Add(slot);
            }
        }

        return slotsDisponiveis;
    }
}