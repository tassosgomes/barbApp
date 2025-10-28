// BarbApp.Application/UseCases/CriarAgendamentoUseCase.cs
using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class CriarAgendamentoUseCase : ICriarAgendamentoUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IBarbeirosRepository _barbeirosRepository;
    private readonly IServicosRepository _servicosRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IDisponibilidadeCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CriarAgendamentoUseCase> _logger;

    public CriarAgendamentoUseCase(
        IAgendamentoRepository agendamentoRepository,
        IBarbeirosRepository barbeirosRepository,
        IServicosRepository servicosRepository,
        IClienteRepository clienteRepository,
        IDisponibilidadeCache cache,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CriarAgendamentoUseCase> logger)
    {
        _agendamentoRepository = agendamentoRepository;
        _barbeirosRepository = barbeirosRepository;
        _servicosRepository = servicosRepository;
        _clienteRepository = clienteRepository;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AgendamentoOutput> Handle(
        Guid clienteId,
        Guid barbeariaId,
        CriarAgendamentoInput input,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Criando agendamento para cliente {ClienteId} na barbearia {BarbeariaId}",
            clienteId, barbeariaId);

        // 1. Validar que cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(clienteId, cancellationToken);
        if (cliente == null)
        {
            _logger.LogWarning("Cliente {ClienteId} não encontrado", clienteId);
            throw new NotFoundException("Cliente não encontrado");
        }

        // 2. Validar que barbeiro existe e está ativo
        var barbeiro = await _barbeirosRepository.GetByIdAsync(input.BarbeiroId, cancellationToken);
        if (barbeiro == null || !barbeiro.IsActive)
        {
            _logger.LogWarning("Barbeiro {BarbeiroId} não encontrado ou inativo", input.BarbeiroId);
            throw new NotFoundException("Barbeiro não encontrado ou inativo");
        }

        // Validar que barbeiro pertence à mesma barbearia
        if (barbeiro.BarbeariaId != barbeariaId)
        {
            _logger.LogWarning(
                "Barbeiro {BarbeiroId} não pertence à barbearia {BarbeariaId}",
                input.BarbeiroId, barbeariaId);
            throw new ForbiddenException("Barbeiro não pertence a esta barbearia");
        }

        // 3. Validar que serviços existem
        var servicos = await _servicosRepository.GetByIdsAsync(input.ServicosIds, cancellationToken);
        if (servicos.Count != input.ServicosIds.Count)
        {
            _logger.LogWarning("Um ou mais serviços não foram encontrados. Solicitados: {ServicosIds}", input.ServicosIds);
            throw new NotFoundException("Um ou mais serviços não foram encontrados");
        }

        // Validar que todos os serviços pertencem à mesma barbearia
        if (servicos.Any(s => s.BarbeariaId != barbeariaId))
        {
            _logger.LogWarning(
                "Serviços não pertencem à barbearia {BarbeariaId}. Serviços: {ServicosIds}",
                barbeariaId, input.ServicosIds);
            throw new ForbiddenException("Serviços não pertencem a esta barbearia");
        }

        // Calcular duração total
        var duracaoTotal = servicos.Sum(s => s.DurationMinutes);

        // 4. Validar que horário está no futuro
        if (input.DataHora <= DateTime.UtcNow)
        {
            _logger.LogWarning("Data/hora {DataHora} não é futura", input.DataHora);
            throw new ValidationException("Data e hora devem ser futuras");
        }

        // 5. Validar que horário não ultrapassa horário de fechamento
        var horarioTermino = input.DataHora.AddMinutes(duracaoTotal);
        if (horarioTermino.Hour >= 20)
        {
            _logger.LogWarning(
                "Agendamento ultrapassa horário de fechamento. Início: {Inicio}, Término: {Termino}",
                input.DataHora, horarioTermino);
            throw new ValidationException("Agendamento ultrapassa horário de fechamento (20:00)");
        }

        // 6. Verificar conflito de horários
        var temConflito = await _agendamentoRepository.ExisteConflito(
            input.BarbeiroId,
            input.DataHora,
            duracaoTotal,
            null,
            cancellationToken);

        if (temConflito)
        {
            _logger.LogWarning(
                "Conflito de horário detectado: Barbeiro {BarbeiroId}, Data {DataHora}",
                input.BarbeiroId,
                input.DataHora);

            throw new HorarioIndisponivelException(
                "Horário não disponível para este barbeiro. Por favor, escolha outro horário.");
        }

        // 7. Criar agendamento
        var agendamento = Agendamento.Create(
            barbeariaId,
            clienteId,
            input.BarbeiroId,
            input.ServicosIds,
            input.DataHora,
            duracaoTotal
        );

        await _agendamentoRepository.AddAsync(agendamento, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation(
            "Agendamento {AgendamentoId} criado com sucesso para cliente {ClienteId}",
            agendamento.Id,
            clienteId);

        // Invalidar cache de disponibilidade
        await _cache.InvalidateAsync(input.BarbeiroId, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30));

        // 8. Retornar output
        return new AgendamentoOutput(
            agendamento.Id,
            _mapper.Map<BarbeiroDto>(barbeiro),
            _mapper.Map<List<ServicoDto>>(servicos),
            agendamento.DataHora,
            agendamento.DuracaoMinutos,
            agendamento.Status.ToString()
        );
    }
}