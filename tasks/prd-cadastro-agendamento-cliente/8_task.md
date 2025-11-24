---
status: completed
parallelizable: false
blocked_by: ["4.0", "5.0", "6.0", "7.0"]
---

<task_context>
<domain>backend/api</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>http_server</dependencies>
<unblocks>9.0, 14.0</unblocks>
</task_context>

# Tarefa 8.0: API - Endpoints Barbeiros/Serviços/Agendamentos (REST + autorização) ✅ CONCLUÍDA

## Visão Geral

Criar todos os endpoints REST para consulta e gestão de agendamentos, incluindo autorização JWT para proteger recursos do cliente. Implementar controllers para: Barbeiros, Serviços, Agendamentos e Disponibilidade.

<requirements>
- BarbeirosController: GET listar, GET disponibilidade
- ServicosController: GET listar
- AgendamentosController: POST criar, GET listar, DELETE cancelar, PUT editar
- Autorização JWT obrigatória (exceto endpoints de auth)
- Validação de tenant: recurso pertence à barbearia do token
- Documentação Swagger completa
- Códigos HTTP corretos: 200, 201, 204, 401, 403, 404, 422
- Rate limiting para prevenção de abuso
- Testes de integração completos
</requirements>

## Subtarefas

- [x] 8.1 Criar BarbeirosController com endpoint GET /api/barbeiros ✅
- [x] 8.2 Criar endpoint GET /api/barbeiros/{id}/disponibilidade ✅
- [x] 8.3 Criar ServicosController com endpoint GET /api/servicos ✅
- [x] 8.4 Criar AgendamentosController vazio ✅
- [x] 8.5 Implementar POST /api/agendamentos ✅
- [x] 8.6 Implementar GET /api/agendamentos/meus ✅
- [x] 8.7 Implementar DELETE /api/agendamentos/{id} ✅
- [x] 8.8 Implementar PUT /api/agendamentos/{id} ✅
- [x] 8.9 Adicionar atributo [Authorize] em todos os controllers (exceto Auth) ✅
- [x] 8.10 Implementar middleware de validação de tenant ✅
- [x] 8.11 Documentar todos os endpoints no Swagger ✅
- [x] 8.12 Adicionar rate limiting (ex: 100 req/min por IP) ✅
- [x] 8.13 Criar testes de integração para todos os endpoints ✅
- [x] 8.14 Testar isolamento multi-tenant (cliente de barbearia A não vê dados de B) ✅

## Detalhes de Implementação

### BarbeirosController

```csharp
[ApiController]
[Route("api/barbeiros")]
[Authorize]
[Produces("application/json")]
public class BarbeirosController : ControllerBase
{
    private readonly IListarBarbeirosUseCase _listarBarbeirosUseCase;
    private readonly IConsultarDisponibilidadeUseCase _consultarDisponibilidadeUseCase;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<BarbeirosController> _logger;

    public BarbeirosController(
        IListarBarbeirosUseCase listarBarbeirosUseCase,
        IConsultarDisponibilidadeUseCase consultarDisponibilidadeUseCase,
        ITenantContext tenantContext,
        ILogger<BarbeirosController> logger)
    {
        _listarBarbeirosUseCase = listarBarbeirosUseCase;
        _consultarDisponibilidadeUseCase = consultarDisponibilidadeUseCase;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Listar barbeiros ativos da barbearia
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BarbeiroDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BarbeiroDto>>> ListarBarbeiros(CancellationToken cancellationToken)
    {
        var barbeariaId = _tenantContext.BarbeariaId;
        var barbeiros = await _listarBarbeirosUseCase.Handle(barbeariaId, cancellationToken);
        return Ok(barbeiros);
    }

    /// <summary>
    /// Consultar disponibilidade de horários de um barbeiro
    /// </summary>
    /// <param name="barbeiroId">ID do barbeiro</param>
    /// <param name="dataInicio">Data de início (formato: yyyy-MM-dd)</param>
    /// <param name="dataFim">Data de fim (formato: yyyy-MM-dd)</param>
    /// <param name="duracaoMinutos">Duração total dos serviços em minutos</param>
    [HttpGet("{barbeiroId}/disponibilidade")]
    [ProducesResponseType(typeof(DisponibilidadeOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DisponibilidadeOutput>> ConsultarDisponibilidade(
        [FromRoute] Guid barbeiroId,
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] int duracaoMinutos,
        CancellationToken cancellationToken)
    {
        var disponibilidade = await _consultarDisponibilidadeUseCase.Handle(
            barbeiroId, 
            dataInicio, 
            dataFim, 
            duracaoMinutos, 
            cancellationToken);
        
        return Ok(disponibilidade);
    }
}
```

### ServicosController

```csharp
[ApiController]
[Route("api/servicos")]
[Authorize]
[Produces("application/json")]
public class ServicosController : ControllerBase
{
    private readonly IListarServicosUseCase _listarServicosUseCase;
    private readonly ITenantContext _tenantContext;

    public ServicosController(
        IListarServicosUseCase listarServicosUseCase,
        ITenantContext tenantContext)
    {
        _listarServicosUseCase = listarServicosUseCase;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Listar serviços oferecidos pela barbearia
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ServicoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServicoDto>>> ListarServicos(CancellationToken cancellationToken)
    {
        var barbeariaId = _tenantContext.BarbeariaId;
        var servicos = await _listarServicosUseCase.Handle(barbeariaId, cancellationToken);
        return Ok(servicos);
    }
}
```

### AgendamentosController

```csharp
[ApiController]
[Route("api/agendamentos")]
[Authorize]
[Produces("application/json")]
public class AgendamentosController : ControllerBase
{
    private readonly ICriarAgendamentoUseCase _criarAgendamentoUseCase;
    private readonly IListarAgendamentosClienteUseCase _listarAgendamentosUseCase;
    private readonly ICancelarAgendamentoUseCase _cancelarAgendamentoUseCase;
    private readonly IEditarAgendamentoUseCase _editarAgendamentoUseCase;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AgendamentosController> _logger;

    public AgendamentosController(
        ICriarAgendamentoUseCase criarAgendamentoUseCase,
        IListarAgendamentosClienteUseCase listarAgendamentosUseCase,
        ICancelarAgendamentoUseCase cancelarAgendamentoUseCase,
        IEditarAgendamentoUseCase editarAgendamentoUseCase,
        ITenantContext tenantContext,
        ILogger<AgendamentosController> logger)
    {
        _criarAgendamentoUseCase = criarAgendamentoUseCase;
        _listarAgendamentosUseCase = listarAgendamentosUseCase;
        _cancelarAgendamentoUseCase = cancelarAgendamentoUseCase;
        _editarAgendamentoUseCase = editarAgendamentoUseCase;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Criar novo agendamento
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AgendamentoOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AgendamentoOutput>> CriarAgendamento(
        [FromBody] CriarAgendamentoInput input,
        CancellationToken cancellationToken)
    {
        var clienteId = ObterClienteIdDoToken();
        var barbeariaId = _tenantContext.BarbeariaId;

        _logger.LogInformation("Cliente {ClienteId} criando agendamento na barbearia {BarbeariaId}", 
            clienteId, barbeariaId);

        var agendamento = await _criarAgendamentoUseCase.Handle(clienteId, barbeariaId, input, cancellationToken);
        
        return CreatedAtAction(nameof(CriarAgendamento), agendamento);
    }

    /// <summary>
    /// Listar agendamentos do cliente autenticado
    /// </summary>
    /// <param name="filtro">Filtro: "proximos" ou "historico"</param>
    [HttpGet("meus")]
    [ProducesResponseType(typeof(List<AgendamentoOutput>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AgendamentoOutput>>> MeusAgendamentos(
        [FromQuery] string filtro = "proximos",
        CancellationToken cancellationToken = default)
    {
        var clienteId = ObterClienteIdDoToken();
        var agendamentos = await _listarAgendamentosUseCase.Handle(clienteId, filtro, cancellationToken);
        return Ok(agendamentos);
    }

    /// <summary>
    /// Cancelar agendamento
    /// </summary>
    [HttpDelete("{agendamentoId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelarAgendamento(
        [FromRoute] Guid agendamentoId,
        CancellationToken cancellationToken)
    {
        var clienteId = ObterClienteIdDoToken();
        
        await _cancelarAgendamentoUseCase.Handle(clienteId, agendamentoId, cancellationToken);
        
        return NoContent();
    }

    /// <summary>
    /// Editar agendamento existente
    /// </summary>
    [HttpPut("{agendamentoId}")]
    [ProducesResponseType(typeof(AgendamentoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AgendamentoOutput>> EditarAgendamento(
        [FromRoute] Guid agendamentoId,
        [FromBody] EditarAgendamentoInput input,
        CancellationToken cancellationToken)
    {
        var clienteId = ObterClienteIdDoToken();
        var barbeariaId = _tenantContext.BarbeariaId;
        
        var agendamento = await _editarAgendamentoUseCase.Handle(
            clienteId, 
            barbeariaId, 
            agendamentoId, 
            input, 
            cancellationToken);
        
        return Ok(agendamento);
    }

    private Guid ObterClienteIdDoToken()
    {
        var clienteIdClaim = User.FindFirst("clienteId")?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(clienteIdClaim, out var clienteId))
        {
            throw new UnauthorizedException("Token inválido");
        }
        
        return clienteId;
    }
}
```

### Rate Limiting Configuration (Program.cs)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 100;
        config.QueueLimit = 0;
    });
});

// No pipeline
app.UseRateLimiter();
```

### Testes de Integração (Isolamento Multi-tenant)

```csharp
[Fact]
public async Task GetBarbeiros_TokenBarbeariaA_DeveRetornarApenasBarbeirosDeBarbeariaA()
{
    // Arrange: Criar 2 barbearias com barbeiros diferentes
    var tokenBarbeariaA = await CadastrarClienteEObterToken("BARBA");
    var tokenBarbeariaB = await CadastrarClienteEObterToken("BARBB");
    
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenBarbeariaA);

    // Act
    var response = await _client.GetAsync("/api/barbeiros");
    var barbeiros = await response.Content.ReadFromJsonAsync<List<BarbeiroDto>>();

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    barbeiros.Should().NotContain(b => b.Nome.Contains("BarbeariaB"));
}

[Fact]
public async Task PostAgendamento_ClienteBarbeariaA_TentaAgendarBarbeiroBarbeariaB_DeveRetornar403()
{
    // Arrange
    var tokenBarbeariaA = await CadastrarClienteEObterToken("BARBA");
    var barbeiroIdBarbeariaB = await ObterBarbeiroIdDaBarbearia("BARBB");
    
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenBarbeariaA);
    
    var request = new CriarAgendamentoInput(barbeiroIdBarbeariaB, new List<Guid> { Guid.NewGuid() }, DateTime.UtcNow.AddDays(1));

    // Act
    var response = await _client.PostAsJsonAsync("/api/agendamentos", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}

[Fact]
public async Task DeleteAgendamento_ClienteNaoProprietario_DeveRetornar403()
{
    // Arrange: Cliente A cria agendamento, Cliente B tenta cancelar
    var tokenClienteA = await CadastrarClienteEObterToken("BARBA");
    var agendamentoId = await CriarAgendamento(tokenClienteA);
    
    var tokenClienteB = await CadastrarClienteEObterToken("BARBA", "Outro Cliente", "11999999999");
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenClienteB);

    // Act
    var response = await _client.DeleteAsync($"/api/agendamentos/{agendamentoId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

## Critérios de Sucesso

- ✅ Todos os endpoints implementados e funcionando
- ✅ Autorização JWT obrigatória (401 sem token)
- ✅ Validação de tenant (403 ao tentar acessar recursos de outra barbearia)
- ✅ Códigos HTTP corretos em todos os cenários
- ✅ Swagger documentado com exemplos
- ✅ Rate limiting configurado e testado
- ✅ Testes de integração para isolamento multi-tenant passando
- ✅ Logs estruturados em todos os endpoints
- ✅ Performance: endpoints respondendo em < 300ms (95º percentil)
