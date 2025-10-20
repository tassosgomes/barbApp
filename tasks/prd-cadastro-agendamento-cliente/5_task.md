---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>backend/application</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>database</dependencies>
<unblocks>8.0</unblocks>
</task_context>

# Tarefa 5.0: Application - Listar Barbeiros e Serviços (consulta)

## Visão Geral

Implementar use cases para consulta de barbeiros e serviços da barbearia. Essas consultas são necessárias para o cliente escolher barbeiro e serviços antes de criar um agendamento.

<requirements>
- Use Case: ListarBarbeirosUseCase (retorna barbeiros ativos da barbearia)
- Use Case: ListarServicosUseCase (retorna serviços ativos da barbearia)
- DTOs: BarbeiroDto, ServicoDto
- Filtros automáticos por barbeariaId (via Global Query Filter)
- Testes unitários para ambos os use cases
- Performance: queries otimizadas sem N+1
</requirements>

## Subtarefas

- [ ] 5.1 Criar DTOs: BarbeiroDto e ServicoDto
- [ ] 5.2 Criar interface IBarbeirosRepository com métodos de consulta
- [ ] 5.3 Implementar BarbeirosRepository
- [ ] 5.4 Criar interface IServicosRepository com métodos de consulta
- [ ] 5.5 Implementar ServicosRepository
- [ ] 5.6 Criar interface IListarBarbeirosUseCase
- [ ] 5.7 Implementar ListarBarbeirosUseCase
- [ ] 5.8 Criar interface IListarServicosUseCase
- [ ] 5.9 Implementar ListarServicosUseCase
- [ ] 5.10 Configurar AutoMapper para Barbeiro → BarbeiroDto
- [ ] 5.11 Configurar AutoMapper para Servico → ServicoDto
- [ ] 5.12 Criar testes unitários para ListarBarbeirosUseCase
- [ ] 5.13 Criar testes unitários para ListarServicosUseCase
- [ ] 5.14 Registrar use cases e repositórios no DI

## Sequenciamento

- **Bloqueado por**: 2.0 (Repositórios)
- **Desbloqueia**: 8.0 (Endpoints de Consulta)
- **Paralelizável**: Sim (independente de autenticação, pode ser feito após 2.0)

## Detalhes de Implementação

### DTOs

```csharp
public record BarbeiroDto(
    Guid Id,
    string Nome,
    string? Foto,
    List<string> Especialidades
);

public record ServicoDto(
    Guid Id,
    string Nome,
    string Descricao,
    int DuracaoMinutos,
    decimal? Preco
);
```

### Interface IBarbeirosRepository

```csharp
public interface IBarbeirosRepository
{
    Task<List<Barbeiro>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<Barbeiro?> GetByIdAsync(Guid barbeiroId, CancellationToken cancellationToken = default);
    Task<bool> EstaAtivoAsync(Guid barbeiroId, CancellationToken cancellationToken = default);
}
```

### Implementação BarbeirosRepository

```csharp
public class BarbeirosRepository : IBarbeirosRepository
{
    private readonly AppDbContext _context;

    public BarbeirosRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Barbeiro>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        // Global Query Filter já filtra por barbeariaId
        return await _context.Barbeiros
            .Where(b => b.Ativo)
            .OrderBy(b => b.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Barbeiro?> GetByIdAsync(Guid barbeiroId, CancellationToken cancellationToken = default)
    {
        return await _context.Barbeiros
            .FirstOrDefaultAsync(b => b.BarbeiroId == barbeiroId, cancellationToken);
    }

    public async Task<bool> EstaAtivoAsync(Guid barbeiroId, CancellationToken cancellationToken = default)
    {
        return await _context.Barbeiros
            .AnyAsync(b => b.BarbeiroId == barbeiroId && b.Ativo, cancellationToken);
    }
}
```

### Interface IServicosRepository

```csharp
public interface IServicosRepository
{
    Task<List<Servico>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<Servico?> GetByIdAsync(Guid servicoId, CancellationToken cancellationToken = default);
    Task<List<Servico>> GetByIdsAsync(List<Guid> servicosIds, CancellationToken cancellationToken = default);
}
```

### Implementação ServicosRepository

```csharp
public class ServicosRepository : IServicosRepository
{
    private readonly AppDbContext _context;

    public ServicosRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Servico>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        // Global Query Filter já filtra por barbeariaId
        return await _context.Servicos
            .Where(s => s.Ativo)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Servico?> GetByIdAsync(Guid servicoId, CancellationToken cancellationToken = default)
    {
        return await _context.Servicos
            .FirstOrDefaultAsync(s => s.ServicoId == servicoId, cancellationToken);
    }

    public async Task<List<Servico>> GetByIdsAsync(List<Guid> servicosIds, CancellationToken cancellationToken = default)
    {
        return await _context.Servicos
            .Where(s => servicosIds.Contains(s.ServicoId))
            .ToListAsync(cancellationToken);
    }
}
```

### Use Case - ListarBarbeirosUseCase

```csharp
public interface IListarBarbeirosUseCase
{
    Task<List<BarbeiroDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default);
}

public class ListarBarbeirosUseCase : IListarBarbeirosUseCase
{
    private readonly IBarbeirosRepository _barbeirosRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListarBarbeirosUseCase> _logger;

    public ListarBarbeirosUseCase(
        IBarbeirosRepository barbeirosRepository,
        IMapper mapper,
        ILogger<ListarBarbeirosUseCase> logger)
    {
        _barbeirosRepository = barbeirosRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<BarbeiroDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listando barbeiros ativos da barbearia {BarbeariaId}", barbeariaId);

        var barbeiros = await _barbeirosRepository.GetAtivosAsync(barbeariaId, cancellationToken);
        
        _logger.LogInformation("Encontrados {Count} barbeiros ativos", barbeiros.Count);

        return _mapper.Map<List<BarbeiroDto>>(barbeiros);
    }
}
```

### Use Case - ListarServicosUseCase

```csharp
public interface IListarServicosUseCase
{
    Task<List<ServicoDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default);
}

public class ListarServicosUseCase : IListarServicosUseCase
{
    private readonly IServicosRepository _servicosRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListarServicosUseCase> _logger;

    public ListarServicosUseCase(
        IServicosRepository servicosRepository,
        IMapper mapper,
        ILogger<ListarServicosUseCase> logger)
    {
        _servicosRepository = servicosRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<ServicoDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listando serviços ativos da barbearia {BarbeariaId}", barbeariaId);

        var servicos = await _servicosRepository.GetAtivosAsync(barbeariaId, cancellationToken);
        
        _logger.LogInformation("Encontrados {Count} serviços ativos", servicos.Count);

        return _mapper.Map<List<ServicoDto>>(servicos);
    }
}
```

### Configuração AutoMapper

```csharp
public class BarbeiroProfile : Profile
{
    public BarbeiroProfile()
    {
        CreateMap<Barbeiro, BarbeiroDto>();
    }
}

public class ServicoProfile : Profile
{
    public ServicoProfile()
    {
        CreateMap<Servico, ServicoDto>();
    }
}
```

### Testes Unitários

```csharp
public class ListarBarbeirosUseCaseTests
{
    private readonly Mock<IBarbeirosRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ListarBarbeirosUseCase>> _loggerMock;
    private readonly ListarBarbeirosUseCase _useCase;

    public ListarBarbeirosUseCaseTests()
    {
        _repositoryMock = new Mock<IBarbeirosRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ListarBarbeirosUseCase>>();
        _useCase = new ListarBarbeirosUseCase(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarApenasBarbeirosAtivos()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barbeiros = new List<Barbeiro>
        {
            new Barbeiro { BarbeiroId = Guid.NewGuid(), Nome = "João", Ativo = true },
            new Barbeiro { BarbeiroId = Guid.NewGuid(), Nome = "Maria", Ativo = true }
        };
        
        _repositoryMock
            .Setup(x => x.GetAtivosAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiros);
        
        _mapperMock
            .Setup(x => x.Map<List<BarbeiroDto>>(It.IsAny<List<Barbeiro>>()))
            .Returns(barbeiros.Select(b => new BarbeiroDto(b.BarbeiroId, b.Nome, null, new List<string>())).ToList());

        // Act
        var result = await _useCase.Handle(barbeariaId);

        // Assert
        Assert.Equal(2, result.Count);
        _repositoryMock.Verify(x => x.GetAtivosAsync(barbeariaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SemBarbeiros_DeveRetornarListaVazia()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        
        _repositoryMock
            .Setup(x => x.GetAtivosAsync(barbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Barbeiro>());
        
        _mapperMock
            .Setup(x => x.Map<List<BarbeiroDto>>(It.IsAny<List<Barbeiro>>()))
            .Returns(new List<BarbeiroDto>());

        // Act
        var result = await _useCase.Handle(barbeariaId);

        // Assert
        Assert.Empty(result);
    }
}

public class ListarServicosUseCaseTests
{
    // Similar structure with tests for:
    // - Handle_DeveRetornarApenasServicosAtivos
    // - Handle_SemServicos_DeveRetornarListaVazia
    // - Handle_DeveOrdenarPorNome
}
```

## Critérios de Sucesso

- ✅ ListarBarbeirosUseCase implementado e retornando apenas ativos
- ✅ ListarServicosUseCase implementado e retornando apenas ativos
- ✅ Repositórios implementados com queries otimizadas
- ✅ Global Query Filter filtrando automaticamente por barbeariaId
- ✅ AutoMapper configurado para Barbeiro e Servico
- ✅ Testes unitários com cobertura > 85%
- ✅ Logs estruturados implementados
- ✅ Use cases registrados no DI
- ✅ Todos os testes passando
- ✅ Sem N+1 queries (verificar com profiler)
