// BarbApp.Application/UseCases/CriarAgendamentoUseCase.cs
using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BarbApp.Application.UseCases;

public class CriarAgendamentoUseCase : ICriarAgendamentoUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IBarbeirosRepository _barbeirosRepository;
    private readonly IServicosRepository _servicosRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDisponibilidadeCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CriarAgendamentoUseCase> _logger;

    public CriarAgendamentoUseCase(
        IAgendamentoRepository agendamentoRepository,
        IBarbeirosRepository barbeirosRepository,
        IServicosRepository servicosRepository,
        ICustomerRepository customerRepository,
        IDisponibilidadeCache cache,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CriarAgendamentoUseCase> logger)
    {
        _agendamentoRepository = agendamentoRepository;
        _barbeirosRepository = barbeirosRepository;
        _servicosRepository = servicosRepository;
        _customerRepository = customerRepository;
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
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation(
            "Iniciando criação de agendamento. ClienteId: {ClienteId}, BarbeariaId: {BarbeariaId}, BarbeiroId: {BarbeiroId}, DataHora: {DataHora}",
            clienteId, barbeariaId, input.BarbeiroId, input.DataHora);

        try
        {
            // 1. Validar que cliente existe
            var customer = await _customerRepository.GetByIdAsync(clienteId, cancellationToken);
            if (customer == null)
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
                    "Conflito de horário detectado. BarbeiroId: {BarbeiroId}, DataHora: {DataHora}",
                    input.BarbeiroId,
                    input.DataHora);

                // Incrementar métrica de conflitos
                BarbAppMetrics.AgendamentosConflitoCounter.WithLabels(barbeariaId.ToString()).Inc();

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

            stopwatch.Stop();

            _logger.LogInformation(
                "Agendamento {AgendamentoId} criado com sucesso. Cliente: {ClienteId}, Barbeiro: {BarbeiroId}, Duração: {Duracao}min, TempoExecucao: {TempoMs}ms",
                agendamento.Id,
                clienteId,
                input.BarbeiroId,
                duracaoTotal,
                stopwatch.ElapsedMilliseconds);

            // Incrementar métricas de sucesso
            BarbAppMetrics.AgendamentosCriadosCounter.WithLabels(barbeariaId.ToString(), "sucesso").Inc();
            BarbAppMetrics.AgendamentoLatenciaHistogram.WithLabels(barbeariaId.ToString()).Observe(stopwatch.Elapsed.TotalSeconds);

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
        catch (HorarioIndisponivelException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao criar agendamento. ClienteId: {ClienteId}, BarbeariaId: {BarbeariaId}",
                clienteId, barbeariaId);
            
            BarbAppMetrics.ErrosCounter.WithLabels("agendamento_criacao", "criar-agendamento").Inc();
            throw;
        }
    }
}