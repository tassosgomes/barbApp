// BarbApp.Application/UseCases/CancelarAgendamentoUseCase.cs
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class CancelarAgendamentoUseCase : ICancelarAgendamentoUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDisponibilidadeCache _cache;
    private readonly ILogger<CancelarAgendamentoUseCase> _logger;

    public CancelarAgendamentoUseCase(
        IAgendamentoRepository agendamentoRepository,
        IUnitOfWork unitOfWork,
        IDisponibilidadeCache cache,
        ILogger<CancelarAgendamentoUseCase> logger)
    {
        _agendamentoRepository = agendamentoRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(Guid clienteId, Guid agendamentoId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cancelando agendamento {AgendamentoId} pelo cliente {ClienteId}",
            agendamentoId, clienteId);

        // 1. Buscar agendamento
        var agendamento = await _agendamentoRepository.GetByIdAsync(agendamentoId, cancellationToken);
        if (agendamento == null)
        {
            _logger.LogWarning("Agendamento {AgendamentoId} não encontrado", agendamentoId);
            throw new NotFoundException("Agendamento não encontrado");
        }

        // 2. Validar que agendamento pertence ao cliente
        if (agendamento.ClienteId != clienteId)
        {
            _logger.LogWarning(
                "Cliente {ClienteId} tentou cancelar agendamento {AgendamentoId} que pertence a outro cliente",
                clienteId, agendamentoId);
            throw new ForbiddenException("Você não tem permissão para cancelar este agendamento");
        }

        // 3. Cancelar (validações internas da entidade)
        agendamento.Cancelar(); // Lança exceção se não puder cancelar

        // 4. Persistir
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation(
            "Agendamento {AgendamentoId} cancelado pelo cliente {ClienteId}",
            agendamentoId, clienteId);

        // Invalidar cache de disponibilidade
        await _cache.InvalidateAsync(agendamento.BarbeiroId, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30));
    }
}