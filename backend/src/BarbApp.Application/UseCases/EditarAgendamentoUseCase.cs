// BarbApp.Application/UseCases/EditarAgendamentoUseCase.cs
using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class EditarAgendamentoUseCase : IEditarAgendamentoUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IBarbeirosRepository _barbeirosRepository;
    private readonly IServicosRepository _servicosRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IDisponibilidadeCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EditarAgendamentoUseCase> _logger;

    public EditarAgendamentoUseCase(
        IAgendamentoRepository agendamentoRepository,
        IBarbeirosRepository barbeirosRepository,
        IServicosRepository servicosRepository,
        IClienteRepository clienteRepository,
        IDisponibilidadeCache cache,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<EditarAgendamentoUseCase> logger)
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
        EditarAgendamentoInput input,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Editando agendamento {AgendamentoId} pelo cliente {ClienteId}",
            input.AgendamentoId, clienteId);

        // 1. Buscar agendamento
        var agendamento = await _agendamentoRepository.GetByIdAsync(input.AgendamentoId, cancellationToken);
        if (agendamento == null)
        {
            _logger.LogWarning("Agendamento {AgendamentoId} não encontrado", input.AgendamentoId);
            throw new NotFoundException("Agendamento não encontrado");
        }

        // 2. Validar que agendamento pertence ao cliente
        if (agendamento.ClienteId != clienteId)
        {
            _logger.LogWarning(
                "Cliente {ClienteId} tentou editar agendamento {AgendamentoId} que pertence a outro cliente",
                clienteId, input.AgendamentoId);
            throw new ForbiddenException("Você não tem permissão para editar este agendamento");
        }

        // 3. Validar que agendamento pertence à barbearia
        if (agendamento.BarbeariaId != barbeariaId)
        {
            _logger.LogWarning(
                "Agendamento {AgendamentoId} não pertence à barbearia {BarbeariaId}",
                input.AgendamentoId, barbeariaId);
            throw new ForbiddenException("Agendamento não pertence a esta barbearia");
        }

        // 4. Preparar novos valores
        var novoBarbeiroId = input.BarbeiroId ?? agendamento.BarbeiroId;
        var novaDataHora = input.DataHora ?? agendamento.DataHora;

        // 5. Se barbeiro mudou, validar que existe e está ativo
        Barber? novoBarbeiro = null;
        if (input.BarbeiroId.HasValue && input.BarbeiroId.Value != agendamento.BarbeiroId)
        {
            novoBarbeiro = await _barbeirosRepository.GetByIdAsync(input.BarbeiroId.Value, cancellationToken);
            if (novoBarbeiro == null || !novoBarbeiro.IsActive)
            {
                _logger.LogWarning("Novo barbeiro {BarbeiroId} não encontrado ou inativo", input.BarbeiroId.Value);
                throw new NotFoundException("Barbeiro não encontrado ou inativo");
            }

            if (novoBarbeiro.BarbeariaId != barbeariaId)
            {
                _logger.LogWarning(
                    "Novo barbeiro {BarbeiroId} não pertence à barbearia {BarbeariaId}",
                    input.BarbeiroId.Value, barbeariaId);
                throw new ForbiddenException("Barbeiro não pertence a esta barbearia");
            }
        }

        // 6. Se serviços mudaram, validar que existem
        List<BarbershopService>? novosServicos = null;
        if (input.ServicosIds != null && input.ServicosIds.Any())
        {
            novosServicos = await _servicosRepository.GetByIdsAsync(input.ServicosIds, cancellationToken);
            if (novosServicos.Count != input.ServicosIds.Count)
            {
                _logger.LogWarning("Um ou mais novos serviços não foram encontrados. Solicitados: {ServicosIds}", input.ServicosIds);
                throw new NotFoundException("Um ou mais serviços não foram encontrados");
            }

            if (novosServicos.Any(s => s.BarbeariaId != barbeariaId))
            {
                _logger.LogWarning(
                    "Novos serviços não pertencem à barbearia {BarbeariaId}. Serviços: {ServicosIds}",
                    barbeariaId, input.ServicosIds);
                throw new ForbiddenException("Serviços não pertencem a esta barbearia");
            }
        }

        // 7. Calcular duração
        var duracaoMinutos = novosServicos?.Sum(s => s.DurationMinutes) ?? agendamento.DuracaoMinutos;

        // 8. Validar horário
        if (input.DataHora.HasValue && input.DataHora.Value <= DateTime.UtcNow)
        {
            _logger.LogWarning("Nova data/hora {DataHora} não é futura", input.DataHora.Value);
            throw new ValidationException("Data e hora devem ser futuras");
        }

        // 9. Validar que horário não ultrapassa horário de fechamento
        var horarioTermino = novaDataHora.AddMinutes(duracaoMinutos);
        if (horarioTermino.Hour >= 20)
        {
            _logger.LogWarning(
                "Edição resultaria em agendamento que ultrapassa horário de fechamento. Início: {Inicio}, Término: {Termino}",
                novaDataHora, horarioTermino);
            throw new ValidationException("Agendamento ultrapassa horário de fechamento (20:00)");
        }

        // 10. Verificar conflito de horários (se horário ou barbeiro mudou)
        if (input.DataHora.HasValue || input.BarbeiroId.HasValue || input.ServicosIds != null)
        {
            var temConflito = await _agendamentoRepository.ExisteConflito(
                novoBarbeiroId,
                novaDataHora,
                duracaoMinutos,
                agendamento.Id, // Ignorar o próprio agendamento
                cancellationToken);

            if (temConflito)
            {
                _logger.LogWarning(
                    "Conflito de horário detectado na edição: Barbeiro {BarbeiroId}, Data {DataHora}",
                    novoBarbeiroId, novaDataHora);

                throw new HorarioIndisponivelException(
                    "Horário não disponível para este barbeiro. Por favor, escolha outro horário.");
            }
        }

        // 11. Atualizar agendamento
        agendamento.Atualizar(
            input.BarbeiroId,
            input.ServicosIds,
            input.DataHora,
            novosServicos?.Sum(s => s.DurationMinutes)
        );

        await _agendamentoRepository.UpdateAsync(agendamento, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation(
            "Agendamento {AgendamentoId} editado com sucesso pelo cliente {ClienteId}",
            input.AgendamentoId, clienteId);

        // Invalidar cache de disponibilidade
        await _cache.InvalidateAsync(agendamento.BarbeiroId, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30));
        if (input.BarbeiroId.HasValue && input.BarbeiroId.Value != agendamento.BarbeiroId)
        {
            await _cache.InvalidateAsync(input.BarbeiroId.Value, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30));
        }

        // 12. Retornar output
        var barbeiroParaOutput = novoBarbeiro ?? agendamento.Barbeiro;
        var servicosParaOutput = novosServicos ?? agendamento.AgendamentoServicos
            .Select(ag => ag.Servico)
            .Where(s => s != null)
            .Select(s => s!)
            .ToList();

        return new AgendamentoOutput(
            agendamento.Id,
            barbeiroParaOutput != null ? _mapper.Map<BarbeiroDto>(barbeiroParaOutput) : null!,
            _mapper.Map<List<ServicoDto>>(servicosParaOutput),
            agendamento.DataHora,
            agendamento.DuracaoMinutos,
            agendamento.Status.ToString()
        );
    }
}