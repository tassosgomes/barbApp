---
status: pending
parallelizable: false
blocked_by: ["2.0", "5.0"]
---

<task_context>
<domain>backend/application</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
<unblocks>7.0, 8.0</unblocks>
</task_context>

# Tarefa 6.0: Application - Algoritmo de Disponibilidade (+ Cache 5min)

## Visão Geral

Implementar o algoritmo crítico de cálculo de disponibilidade de horários para agendamentos. Este algoritmo deve:
- Calcular slots de 30 minutos entre 08:00 e 20:00
- Remover horários já ocupados (considerando duração dos serviços)
- Considerar apenas agendamentos Pendentes e Confirmados
- Implementar cache em memória com TTL de 5 minutos para performance

<requirements>
- Use Case: ConsultarDisponibilidadeUseCase
- Algoritmo de cálculo de sobreposição de horários
- Cache IMemoryCache com TTL de 5 minutos
- DTOs: DisponibilidadeOutput, DiaDisponivel, HorarioDisponivel
- Testes unitários cobrindo todos os cenários de sobreposição
- Performance: query otimizada para buscar agendamentos de período
</requirements>

## Subtarefas

- [ ] 6.1 Criar DTOs: DisponibilidadeOutput, DiaDisponivel
- [ ] 6.2 Criar interface IDisponibilidadeCache
- [ ] 6.3 Implementar DisponibilidadeCache com IMemoryCache
- [ ] 6.4 Criar interface IConsultarDisponibilidadeUseCase
- [ ] 6.5 Implementar algoritmo de geração de slots de 30min (08:00-20:00)
- [ ] 6.6 Implementar algoritmo de detecção de sobreposição
- [ ] 6.7 Implementar ConsultarDisponibilidadeUseCase com cache
- [ ] 6.8 Criar testes unitários para algoritmo de sobreposição
- [ ] 6.9 Criar testes unitários para cache (hit/miss)
- [ ] 6.10 Testar cenários: sem agendamentos, múltiplos agendamentos, agendamento de 60min
- [ ] 6.11 Adicionar métricas de performance (cache hit rate)
- [ ] 6.12 Registrar use case e cache no DI

## Detalhes de Implementação

### DTOs

```csharp
public record DisponibilidadeOutput(
    BarbeiroDto Barbeiro,
    List<DiaDisponivel> DiasDisponiveis
);

public record DiaDisponivel(
    DateTime Data,
    List<string> HorariosDisponiveis // ["09:00", "09:30", "10:00"]
);
```

### Interface e Implementação Cache

```csharp
public interface IDisponibilidadeCache
{
    Task<DisponibilidadeOutput?> GetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim);
    Task SetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, DisponibilidadeOutput disponibilidade);
}

public class DisponibilidadeCache : IDisponibilidadeCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

    public DisponibilidadeCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<DisponibilidadeOutput?> GetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        _cache.TryGetValue(key, out DisponibilidadeOutput? disponibilidade);
        return Task.FromResult(disponibilidade);
    }

    public Task SetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, DisponibilidadeOutput disponibilidade)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        _cache.Set(key, disponibilidade, _ttl);
        return Task.CompletedTask;
    }

    private string GerarChave(Guid barbeiroId, DateTime dataInicio, DateTime dataFim)
    {
        return $"disponibilidade:{barbeiroId}:{dataInicio:yyyyMMdd}:{dataFim:yyyyMMdd}";
    }
}
```

### Use Case - ConsultarDisponibilidadeUseCase

```csharp
public interface IConsultarDisponibilidadeUseCase
{
    Task<DisponibilidadeOutput> Handle(
        Guid barbeiroId, 
        DateTime dataInicio, 
        DateTime dataFim, 
        int duracaoServicosMinutos,
        CancellationToken cancellationToken = default);
}

public class ConsultarDisponibilidadeUseCase : IConsultarDisponibilidadeUseCase
{
    private readonly IBarbeirosRepository _barbeirosRepository;
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IDisponibilidadeCache _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<ConsultarDisponibilidadeUseCase> _logger;

    private const int SLOT_MINUTOS = 30;
    private const int HORA_INICIO = 8;
    private const int HORA_FIM = 20;

    public ConsultarDisponibilidadeUseCase(
        IBarbeirosRepository barbeirosRepository,
        IAgendamentoRepository agendamentoRepository,
        IDisponibilidadeCache cache,
        IMapper mapper,
        ILogger<ConsultarDisponibilidadeUseCase> logger)
    {
        _barbeirosRepository = barbeirosRepository;
        _agendamentoRepository = agendamentoRepository;
        _cache = cache;
        _mapper = mapper;
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
        var cached = await _cache.GetAsync(barbeiroId, dataInicio, dataFim);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit para disponibilidade do barbeiro {BarbeiroId}", barbeiroId);
            return cached;
        }

        _logger.LogDebug("Cache miss para disponibilidade do barbeiro {BarbeiroId}", barbeiroId);

        // 2. Buscar barbeiro
        var barbeiro = await _barbeirosRepository.GetByIdAsync(barbeiroId, cancellationToken);
        if (barbeiro == null)
        {
            throw new NotFoundException($"Barbeiro {barbeiroId} não encontrado");
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
            _mapper.Map<BarbeiroDto>(barbeiro),
            diasDisponiveis
        );

        // 6. Salvar no cache
        await _cache.SetAsync(barbeiroId, dataInicio, dataFim, output);

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
                var agendamentoTermino = a.HorarioTermino;
                
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
```

### Testes Unitários (Cenários Críticos)

```csharp
public class ConsultarDisponibilidadeUseCaseTests
{
    [Fact]
    public async Task Handle_SemAgendamentos_DeveRetornarTodosHorariosDisponiveis()
    {
        // Arrange: Barbeiro sem agendamentos
        // Act: Consultar disponibilidade
        // Assert: Deve retornar todos os slots de 08:00 a 20:00
    }

    [Fact]
    public async Task Handle_ComAgendamentoDe30Min_DeveBlocarApenasUmSlot()
    {
        // Arrange: Agendamento de 30min às 10:00
        // Act: Consultar disponibilidade
        // Assert: Slot 10:00 bloqueado, 09:30 e 10:30 disponíveis
    }

    [Fact]
    public async Task Handle_ComAgendamentoDe60Min_DeveBlocarDoisSlots()
    {
        // Arrange: Agendamento de 60min às 10:00
        // Act: Consultar disponibilidade
        // Assert: Slots 10:00 e 10:30 bloqueados
    }

    [Fact]
    public async Task Handle_AgendamentoCancelado_NaoDeveBloquerHorario()
    {
        // Arrange: Agendamento cancelado às 10:00
        // Act: Consultar disponibilidade
        // Assert: Slot 10:00 disponível
    }

    [Fact]
    public async Task Handle_DataHoje_DeveRemoverHorariosPassados()
    {
        // Arrange: Consultar disponibilidade para hoje às 14:00
        // Act: Consultar disponibilidade
        // Assert: Horários antes de 14:00 não aparecem
    }

    [Fact]
    public async Task Handle_ComCache_DeveRetornarDoCacheSemConsultarBanco()
    {
        // Arrange: Cache populado
        // Act: Segunda consulta
        // Assert: Repository não deve ser chamado
    }
}
```

## Critérios de Sucesso

- ✅ Algoritmo de disponibilidade calculando corretamente sobreposições
- ✅ Cache implementado com TTL de 5 minutos
- ✅ Cenários de teste cobrindo: sem agendamentos, 30min, 60min, cancelados, horários passados
- ✅ Performance: consulta de 7 dias deve executar em < 500ms
- ✅ Cache hit rate > 80% em cenários de uso real
- ✅ Testes unitários com cobertura > 95%
- ✅ Logs estruturados para debug de cache
- ✅ Todos os testes passando
