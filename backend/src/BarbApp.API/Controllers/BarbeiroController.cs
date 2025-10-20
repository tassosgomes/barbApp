using BarbApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BarbApp.API.Controllers;

/// <summary>
/// Endpoints relacionados ao barbeiro autenticado (seleção de contexto)
/// </summary>
[ApiController]
[Route("api/barbeiro")]
[Authorize(Roles = "Barbeiro")]
[Produces("application/json")]
public class BarbeiroController : ControllerBase
{
    private readonly ILogger<BarbeiroController> _logger;

    public BarbeiroController(ILogger<BarbeiroController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista as barbearias às quais o barbeiro autenticado está vinculado
    /// </summary>
    /// <returns>Lista de barbearias (vínculos) do barbeiro</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não possui permissão</response>
    [HttpGet("barbearias")]
    [ProducesResponseType(typeof(IEnumerable<BarbershopLinkOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BarbershopLinkOutput>>> ListarBarbearias()
    {
        _logger.LogInformation("[Stub] Listando barbearias vinculadas ao barbeiro autenticado");

        // TODO: Implementar listagem real a partir do repositório
        // Por enquanto, retornamos 501 para indicar que o endpoint está em implementação
        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "Endpoint em implementação" });
    }
}

