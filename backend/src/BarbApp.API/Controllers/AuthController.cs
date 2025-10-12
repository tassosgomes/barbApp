using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticateAdminCentralUseCase _authenticateAdminCentral;
    private readonly IAuthenticateAdminBarbeariaUseCase _authenticateAdminBarbearia;
    private readonly IAuthenticateBarbeiroUseCase _authenticateBarbeiro;
    private readonly IAuthenticateClienteUseCase _authenticateCliente;
    private readonly IListBarbeirosBarbeariaUseCase _listBarbeiros;
    private readonly ITrocarContextoBarbeiroUseCase _trocarContexto;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticateAdminCentralUseCase authenticateAdminCentral,
        IAuthenticateAdminBarbeariaUseCase authenticateAdminBarbearia,
        IAuthenticateBarbeiroUseCase authenticateBarbeiro,
        IAuthenticateClienteUseCase authenticateCliente,
        IListBarbeirosBarbeariaUseCase listBarbeiros,
        ITrocarContextoBarbeiroUseCase trocarContexto,
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
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAdminCentral([FromBody] LoginAdminCentralInput input)
    {
        _logger.LogInformation("Admin Central login attempt for email: {Email}", input.Email);

        var response = await _authenticateAdminCentral.ExecuteAsync(input);

        _logger.LogInformation("Admin Central login successful for email: {Email}", input.Email);

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
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAdminBarbearia([FromBody] LoginAdminBarbeariaInput input)
    {
        _logger.LogInformation(
            "Admin Barbearia login attempt for email: {Email}, Codigo: {Codigo}",
            input.Email,
            input.Codigo);

        var response = await _authenticateAdminBarbearia.ExecuteAsync(input);

        _logger.LogInformation("Admin Barbearia login successful for email: {Email}", input.Email);

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
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginBarbeiro([FromBody] LoginBarbeiroInput input)
    {
        _logger.LogInformation(
            "Barbeiro login attempt for telefone: {Telefone}, Codigo: {Codigo}",
            input.Telefone,
            input.Codigo);

        var response = await _authenticateBarbeiro.ExecuteAsync(input);

        _logger.LogInformation("Barbeiro login successful for telefone: {Telefone}", input.Telefone);

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
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginCliente([FromBody] LoginClienteInput input)
    {
        _logger.LogInformation("Cliente login attempt for telefone: {Telefone}", input.Telefone);

        var response = await _authenticateCliente.ExecuteAsync(input);

        _logger.LogInformation("Cliente login successful for telefone: {Telefone}", input.Telefone);

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
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
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
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponse>> TrocarContexto([FromBody] TrocarContextoInput input)
    {
        _logger.LogInformation("Trocar contexto attempt to BarbeariaId: {BarbeariaId}", input.NovaBarbeariaId);

        var response = await _trocarContexto.ExecuteAsync(input);

        _logger.LogInformation("Contexto trocado successfully to BarbeariaId: {BarbeariaId}", input.NovaBarbeariaId);

        return Ok(response);
    }
}
