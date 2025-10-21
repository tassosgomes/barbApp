using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/public/barbershops")]
[Produces("application/json")]
public class PublicLandingPageController : ControllerBase
{
    private readonly ILandingPageService _landingPageService;
    private readonly ILogger<PublicLandingPageController> _logger;

    public PublicLandingPageController(
        ILandingPageService landingPageService,
        ILogger<PublicLandingPageController> logger)
    {
        _landingPageService = landingPageService;
        _logger = logger;
    }

    /// <summary>
    /// Busca landing page pública de uma barbearia pelo código
    /// </summary>
    /// <param name="code">Código único da barbearia</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados públicos da landing page</returns>
    /// <response code="200">Landing page retornada com sucesso</response>
    /// <response code="404">Landing page não encontrada</response>
    [HttpGet("{code}/landing-page")]
    [OutputCache(Duration = 300)] // 5 minutos
    [ProducesResponseType(typeof(PublicLandingPageOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicLandingPage(
        string code,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando landing page pública para código: {Code}", code);

        try
        {
            var data = await _landingPageService.GetPublicByCodeAsync(code, cancellationToken);
            return Ok(data);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Landing page não encontrada para código: {Code}. Error: {Error}", code, ex.Message);
            return NotFound(new { error = "Landing page não encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar landing page pública para código: {Code}", code);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}