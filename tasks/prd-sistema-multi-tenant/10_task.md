---
status: completed
parallelizable: false
blocked_by: ["7.0", "6.0"]
---

<task_context>
<domain>Camada de Apresentação</domain>
<type>Implementação</type>
<scope>API Controllers</scope>
<complexity>média</complexity>
<dependencies>ASP.NET Core, Use Cases, DTOs</dependencies>
<unblocks>"11.0"</unblocks>
</task_context>

# Tarefa 10.0: Implementar Controller de Autenticação

## Visão Geral
Implementar o AuthController com 5 endpoints REST para autenticação de diferentes tipos de usuários, listagem de barbeiros e troca de contexto de tenant.

<requirements>
- POST /api/auth/admin-central/login
- POST /api/auth/admin-barbearia/login
- POST /api/auth/barbeiro/login
- POST /api/auth/cliente/login
- GET /api/auth/barbeiros (requer autenticação)
- POST /api/auth/barbeiro/trocar-contexto (requer autenticação)
- Validação automática de DTOs com FluentValidation
- Tratamento de erros apropriado
- Documentação OpenAPI/Swagger
</requirements>

## Subtarefas
- [ ] 10.1 Criar AuthController com estrutura base
- [ ] 10.2 Implementar endpoint de login AdminCentral
- [ ] 10.3 Implementar endpoint de login AdminBarbearia
- [ ] 10.4 Implementar endpoint de login Barbeiro
- [ ] 10.5 Implementar endpoint de login Cliente
- [ ] 10.6 Implementar endpoint de listagem de barbeiros
- [ ] 10.7 Implementar endpoint de troca de contexto
- [ ] 10.8 Adicionar atributos de documentação Swagger
- [ ] 10.9 Criar testes de integração para todos os endpoints

## Sequenciamento
- **Bloqueado por**: 7.0 (Use Cases), 6.0 (DTOs)
- **Desbloqueia**: 11.0 (Configurar API)
- **Paralelizável**: Não (depende de use cases e DTOs)

## Detalhes de Implementação

### AuthController

```csharp
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthenticateAdminCentralUseCase _authenticateAdminCentral;
    private readonly AuthenticateAdminBarbeariaUseCase _authenticateAdminBarbearia;
    private readonly AuthenticateBarbeiroUseCase _authenticateBarbeiro;
    private readonly AuthenticateClienteUseCase _authenticateCliente;
    private readonly ListBarbeirosBarbeariaUseCase _listBarbeiros;
    private readonly TrocarContextoBarbeiroUseCase _trocarContexto;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthenticateAdminCentralUseCase authenticateAdminCentral,
        AuthenticateAdminBarbeariaUseCase authenticateAdminBarbearia,
        AuthenticateBarbeiroUseCase authenticateBarbeiro,
        AuthenticateClienteUseCase authenticateCliente,
        ListBarbeirosBarbeariaUseCase listBarbeiros,
        TrocarContextoBarbeiroUseCase trocarContexto,
        ILogger<AuthController> logger)
    {
        _authenticateAdminCentral = authenticateAdminCentral;
        _authenticateAdminBarbearia = authenticateAdminBarbearia;
        _authenticateBarbeiro = authenticateBarbeiro;
        _authenticateCliente = authenticateCliente;
        _listBarbeiros = listBarbeiros;
        _trocarContexto = trocarContexto;
        _logger = logger;
    }

    /// <summary>
    /// Autentica um administrador central
    /// </summary>
    /// <param name="input">Credenciais do administrador central</param>
    /// <returns>Token JWT e informações do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("admin-central/login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAdminCentral(
        [FromBody] LoginAdminCentralInput input)
    {
        _logger.LogInformation(
            "Admin Central login attempt for email: {Email}",
            input.Email);

        var response = await _authenticateAdminCentral.ExecuteAsync(input);

        _logger.LogInformation(
            "Admin Central login successful for email: {Email}",
            input.Email);

        return Ok(response);
    }

    /// <summary>
    /// Autentica um administrador de barbearia
    /// </summary>
    /// <param name="input">Credenciais do administrador de barbearia</param>
    /// <returns>Token JWT e informações do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("admin-barbearia/login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAdminBarbearia(
        [FromBody] LoginAdminBarbeariaInput input)
    {
        _logger.LogInformation(
            "Admin Barbearia login attempt for email: {Email}, BarbeariaId: {BarbeariaId}",
            input.Email,
            input.BarbeariaId);

        var response = await _authenticateAdminBarbearia.ExecuteAsync(input);

        _logger.LogInformation(
            "Admin Barbearia login successful for email: {Email}",
            input.Email);

        return Ok(response);
    }

    /// <summary>
    /// Autentica um barbeiro
    /// </summary>
    /// <param name="input">Credenciais do barbeiro</param>
    /// <returns>Token JWT e informações do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("barbeiro/login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginBarbeiro(
        [FromBody] LoginBarbeiroInput input)
    {
        _logger.LogInformation(
            "Barbeiro login attempt for email: {Email}, BarbeariaId: {BarbeariaId}",
            input.Email,
            input.BarbeariaId);

        var response = await _authenticateBarbeiro.ExecuteAsync(input);

        _logger.LogInformation(
            "Barbeiro login successful for email: {Email}",
            input.Email);

        return Ok(response);
    }

    /// <summary>
    /// Autentica um cliente
    /// </summary>
    /// <param name="input">Credenciais do cliente</param>
    /// <returns>Token JWT e informações do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("cliente/login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginCliente(
        [FromBody] LoginClienteInput input)
    {
        _logger.LogInformation(
            "Cliente login attempt for email: {Email}",
            input.Email);

        var response = await _authenticateCliente.ExecuteAsync(input);

        _logger.LogInformation(
            "Cliente login successful for email: {Email}",
            input.Email);

        return Ok(response);
    }

    /// <summary>
    /// Lista barbeiros da barbearia do usuário autenticado
    /// </summary>
    /// <returns>Lista de barbeiros da barbearia</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    [HttpGet("barbeiros")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<BarberInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BarberInfo>>> ListBarbeiros()
    {
        _logger.LogInformation("Listing barbeiros for authenticated user");

        var barbeiros = await _listBarbeiros.ExecuteAsync();

        return Ok(barbeiros);
    }

    /// <summary>
    /// Troca o contexto de barbearia para um barbeiro que trabalha em múltiplas barbearias
    /// </summary>
    /// <param name="input">ID da nova barbearia</param>
    /// <returns>Novo token JWT com contexto atualizado</returns>
    /// <response code="200">Contexto trocado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="404">Barbeiro não encontrado na barbearia especificada</response>
    [HttpPost("barbeiro/trocar-contexto")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponse>> TrocarContexto(
        [FromBody] TrocarContextoInput input)
    {
        _logger.LogInformation(
            "Trocar contexto attempt to BarbeariaId: {BarbeariaId}",
            input.NovaBarbeariaId);

        var response = await _trocarContexto.ExecuteAsync(input);

        _logger.LogInformation(
            "Contexto trocado successfully to BarbeariaId: {BarbeariaId}",
            input.NovaBarbeariaId);

        return Ok(response);
    }
}
```

### Configuração de Validação FluentValidation

```csharp
// No Program.cs
public static class ValidationConfiguration
{
    public static IServiceCollection AddFluentValidationConfiguration(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginAdminCentralInputValidator>();

        services.AddFluentValidationAutoValidation(config =>
        {
            config.DisableDataAnnotationsValidation = true;
        });

        return services;
    }
}
```

## Critérios de Sucesso
- ✅ Todos os 5 endpoints implementados e funcionando
- ✅ Validação automática de DTOs com FluentValidation
- ✅ Autorização funciona corretamente em endpoints protegidos
- ✅ Logging apropriado de operações
- ✅ Tratamento de erros retorna status codes corretos
- ✅ Documentação Swagger completa e clara
- ✅ Testes de integração cobrem todos os endpoints
- ✅ Respostas seguem padrão consistente
- ✅ Mensagens de erro são claras e úteis

## Tempo Estimado
**4 horas**

## Referências
- TechSpec: Seção "4.5 Fase 1.5: Controller e Endpoints"
- PRD: Seção "Especificação de Endpoints REST"
- ASP.NET Core Web API Best Practices
