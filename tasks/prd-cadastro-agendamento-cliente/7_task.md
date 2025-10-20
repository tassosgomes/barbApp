---
status: pending
parallelizable: false
blocked_by: ["2.0", "5.0", "6.0"]
---

<task_context>
<domain>backend/application</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
<unblocks>8.0</unblocks>
</task_context>

# Tarefa 7.0: Application - Criar/Cancelar/Listar Agendamentos (com lock otimista)

## Visão Geral

Implementar os use cases críticos de gestão de agendamentos: criação com validação de conflito e lock otimista, cancelamento com validações, listagem de agendamentos do cliente, e edição de agendamentos. Esta é a funcionalidade core do módulo.

<requirements>
- Use Case: CriarAgendamentoUseCase com validação de conflito e lock otimista
- Use Case: CancelarAgendamentoUseCase com validações (não pode cancelar concluído/passado)
- Use Case: EditarAgendamentoUseCase com validação de conflito
- Use Case: ListarAgendamentosClienteUseCase (filtros: próximos/histórico)
- DTOs: CriarAgendamentoInput, AgendamentoOutput, EditarAgendamentoInput
- Validação de conflito OBRIGATÓRIA antes de criar/editar
- Transaction com lock otimista para prevenir race conditions
- Testes unitários cobrindo todos os cenários de conflito
</requirements>

## Subtarefas

- [ ] 7.1 Criar DTOs: CriarAgendamentoInput, AgendamentoOutput, EditarAgendamentoInput
- [ ] 7.2 Criar validador FluentValidation para CriarAgendamentoInput
- [ ] 7.3 Criar interface ICriarAgendamentoUseCase
- [ ] 7.4 Implementar algoritmo de validação de conflito de horários
- [ ] 7.5 Implementar CriarAgendamentoUseCase com transaction e lock otimista
- [ ] 7.6 Criar interface ICancelarAgendamentoUseCase
- [ ] 7.7 Implementar CancelarAgendamentoUseCase com validações
- [ ] 7.8 Criar interface IEditarAgendamentoUseCase
- [ ] 7.9 Implementar EditarAgendamentoUseCase com validação de conflito
- [ ] 7.10 Criar interface IListarAgendamentosClienteUseCase
- [ ] 7.11 Implementar ListarAgendamentosClienteUseCase com filtros
- [ ] 7.12 Criar exceções: HorarioIndisponivelException, AgendamentoJaExisteException
- [ ] 7.13 Criar testes unitários para CriarAgendamento (sucesso, conflito, race condition)
- [ ] 7.14 Criar testes unitários para CancelarAgendamento (sucesso, não pode cancelar concluído)
- [ ] 7.15 Criar testes unitários para EditarAgendamento (sucesso, conflito)
- [ ] 7.16 Criar testes unitários para ListarAgendamentos (próximos, histórico)
- [ ] 7.17 Invalidar cache de disponibilidade após criar/cancelar/editar
- [ ] 7.18 Registrar use cases no DI

## Detalhes de Implementação

### DTOs

```csharp
public record CriarAgendamentoInput(
    Guid BarbeiroId,
    List<Guid> ServicosIds,
    DateTime DataHora
);

public record EditarAgendamentoInput(
    Guid AgendamentoId,
    Guid? BarbeiroId,
    List<Guid>? ServicosIds,
    DateTime? DataHora
);

public record AgendamentoOutput(
    Guid Id,
    BarbeiroDto Barbeiro,
    List<ServicoDto> Servicos,
    DateTime DataHora,
    int DuracaoTotal,
    string Status
);
```

### Use Case - CriarAgendamentoUseCase (Algoritmo Crítico)

```csharp
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

    public async Task<AgendamentoOutput> Handle(
        Guid clienteId, 
        Guid barbeariaId, 
        CriarAgendamentoInput input, 
        CancellationToken cancellationToken = default)
    {
        // 1. Validar que cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(clienteId, cancellationToken);
        if (cliente == null)
            throw new NotFoundException("Cliente não encontrado");

        // 2. Validar que barbeiro existe e está ativo
        var barbeiro = await _barbeirosRepository.GetByIdAsync(input.BarbeiroId, cancellationToken);
        if (barbeiro == null || !barbeiro.Ativo)
            throw new NotFoundException("Barbeiro não encontrado ou inativo");

        // Validar que barbeiro pertence à mesma barbearia
        if (barbeiro.BarbeariaId != barbeariaId)
            throw new ForbiddenException("Barbeiro não pertence a esta barbearia");

        // 3. Validar que serviços existem e calcular duração total
        var servicos = await _servicosRepository.GetByIdsAsync(input.ServicosIds, cancellationToken);
        if (servicos.Count != input.ServicosIds.Count)
            throw new NotFoundException("Um ou mais serviços não foram encontrados");

        // Validar que todos os serviços pertencem à mesma barbearia
        if (servicos.Any(s => s.BarbeariaId != barbeariaId))
            throw new ForbiddenException("Serviços não pertencem a esta barbearia");

        var duracaoTotal = servicos.Sum(s => s.DuracaoMinutos);

        // 4. Validar que horário está no futuro
        if (input.DataHora < DateTime.UtcNow)
            throw new BusinessException("Data e hora devem ser futuras");

        // 5. Validar que horário não ultrapassa horário de fechamento
        var horarioTermino = input.DataHora.AddMinutes(duracaoTotal);
        if (horarioTermino.Hour >= 20)
            throw new BusinessException("Agendamento ultrapassa horário de fechamento (20:00)");

        // 6. VALIDAÇÃO CRÍTICA: Verificar conflito de horários (DENTRO DE TRANSACTION)
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
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
            var agendamento = new Agendamento(
                barbeariaId,
                clienteId,
                input.BarbeiroId,
                input.ServicosIds,
                input.DataHora,
                duracaoTotal
            );

            await _agendamentoRepository.AddAsync(agendamento, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Agendamento {AgendamentoId} criado com sucesso para cliente {ClienteId}", 
                agendamento.AgendamentoId, 
                clienteId);

            // 8. Invalidar cache de disponibilidade
            await InvalidarCacheDisponibilidade(input.BarbeiroId, input.DataHora);

            // 9. Retornar output
            return new AgendamentoOutput(
                agendamento.AgendamentoId,
                _mapper.Map<BarbeiroDto>(barbeiro),
                _mapper.Map<List<ServicoDto>>(servicos),
                agendamento.DataHora,
                agendamento.DuracaoTotal,
                agendamento.Status.ToString()
            );
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task InvalidarCacheDisponibilidade(Guid barbeiroId, DateTime dataHora)
    {
        // Cache usa chave com data, então invalidar para o dia do agendamento
        // Implementação depende de como DisponibilidadeCache expõe método de invalidação
    }
}
```

### Validação de Conflito (Repositório)

```csharp
public async Task<bool> ExisteConflito(
    Guid barbeiroId, 
    DateTime dataHora, 
    int duracaoMinutos, 
    Guid? agendamentoIdParaIgnorar = null, 
    CancellationToken cancellationToken = default)
{
    var horarioTermino = dataHora.AddMinutes(duracaoMinutos);

    var query = _context.Agendamentos
        .Where(a => a.BarbeiroId == barbeiroId)
        .Where(a => a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado)
        .Where(a => a.DataHora < horarioTermino && a.HorarioTermino > dataHora); // Lógica de sobreposição

    if (agendamentoIdParaIgnorar.HasValue)
    {
        query = query.Where(a => a.AgendamentoId != agendamentoIdParaIgnorar.Value);
    }

    return await query.AnyAsync(cancellationToken);
}
```

### Use Case - CancelarAgendamentoUseCase

```csharp
public class CancelarAgendamentoUseCase : ICancelarAgendamentoUseCase
{
    public async Task Handle(Guid clienteId, Guid agendamentoId, CancellationToken cancellationToken = default)
    {
        // 1. Buscar agendamento
        var agendamento = await _agendamentoRepository.GetByIdAsync(agendamentoId, cancellationToken);
        if (agendamento == null)
            throw new NotFoundException("Agendamento não encontrado");

        // 2. Validar que agendamento pertence ao cliente
        if (agendamento.ClienteId != clienteId)
            throw new ForbiddenException("Você não tem permissão para cancelar este agendamento");

        // 3. Cancelar (validações internas da entidade)
        agendamento.Cancelar(); // Lança exceção se não puder cancelar

        // 4. Persistir
        await _unitOfWork.CommitAsync(cancellationToken);

        // 5. Invalidar cache
        await InvalidarCacheDisponibilidade(agendamento.BarbeiroId, agendamento.DataHora);

        _logger.LogInformation("Agendamento {AgendamentoId} cancelado pelo cliente {ClienteId}", 
            agendamentoId, clienteId);
    }
}
```

### Use Case - ListarAgendamentosClienteUseCase

```csharp
public class ListarAgendamentosClienteUseCase : IListarAgendamentosClienteUseCase
{
    public async Task<List<AgendamentoOutput>> Handle(
        Guid clienteId, 
        string filtro, // "proximos" ou "historico"
        CancellationToken cancellationToken = default)
    {
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

        // Filtrar por data
        if (filtro.ToLower() == "proximos")
        {
            agendamentos = agendamentos
                .Where(a => a.DataHora >= DateTime.UtcNow && 
                           (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado))
                .OrderBy(a => a.DataHora)
                .ToList();
        }
        else if (filtro.ToLower() == "historico")
        {
            agendamentos = agendamentos
                .Where(a => a.Status == StatusAgendamento.Concluido || a.Status == StatusAgendamento.Cancelado)
                .OrderByDescending(a => a.DataHora)
                .ToList();
        }

        return _mapper.Map<List<AgendamentoOutput>>(agendamentos);
    }
}
```

### Testes Unitários (Cenários Críticos)

```csharp
[Fact]
public async Task CriarAgendamento_ComHorarioDisponivel_DeveCriarComSucesso()
{
    // Arrange: Barbeiro sem conflitos
    // Act: Criar agendamento
    // Assert: Agendamento criado com status Pendente
}

[Fact]
public async Task CriarAgendamento_ComHorarioOcupado_DeveLancarHorarioIndisponivelException()
{
    // Arrange: Barbeiro já tem agendamento no horário
    // Act & Assert: Lança HorarioIndisponivelException
}

[Fact]
public async Task CriarAgendamento_BarbeiroDeOutraBarbearia_DeveLancarForbiddenException()
{
    // Arrange: Token de barbearia A, barbeiro de barbearia B
    // Act & Assert: Lança ForbiddenException
}

[Fact]
public async Task CancelarAgendamento_AgendamentoConcluido_DeveLancarException()
{
    // Arrange: Agendamento com status Concluído
    // Act & Assert: Lança InvalidOperationException
}
```

## Critérios de Sucesso

- ✅ CriarAgendamentoUseCase implementado com validação de conflito OBRIGATÓRIA
- ✅ Transaction com lock otimista previne race conditions (testes de concorrência)
- ✅ CancelarAgendamentoUseCase com todas as validações
- ✅ EditarAgendamentoUseCase com validação de conflito
- ✅ ListarAgendamentosClienteUseCase com filtros funcionando
- ✅ Cache de disponibilidade invalidado após criar/cancelar/editar
- ✅ Testes de concorrência: 2 clientes tentando mesmo horário simultaneamente
- ✅ Cobertura de testes unitários > 90%
- ✅ Todos os testes passando
