using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/admin/landing-pages")]
[Authorize(Roles = "AdminBarbearia,AdminCentral")]
[Produces("application/json")]
public class LandingPagesController : ControllerBase
{
    private readonly ILandingPageService _landingPageService;
    private readonly ILogoUploadService _logoUploadService;
    private readonly ILogger<LandingPagesController> _logger;

    public LandingPagesController(
        ILandingPageService landingPageService,
        ILogoUploadService logoUploadService,
        ILogger<LandingPagesController> logger)
    {
        _landingPageService = landingPageService;
        _logoUploadService = logoUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Busca a configuração da landing page de uma barbearia
    /// </summary>
    /// <param name="barbershopId">ID da barbearia</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Configuração da landing page</returns>
    /// <response code="200">Configuração retornada com sucesso</response>
    /// <response code="401">Token inválido ou expirado</response>
    /// <response code="403">Admin não pertence à barbearia solicitada</response>
    /// <response code="404">Landing page não encontrada</response>
    [HttpGet("{barbershopId:guid}")]
    [ProducesResponseType(typeof(LandingPageConfigOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandingPageConfigOutput>> GetConfig(
        [FromRoute] Guid barbershopId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting landing page config for barbershop: {BarbershopId}", barbershopId);

        if (!IsAuthorizedForBarbershop(barbershopId))
        {
            _logger.LogWarning("Admin unauthorized to access barbershop: {BarbershopId}", barbershopId);
            return Forbid();
        }

        try
        {
            var result = await _landingPageService.GetByBarbershopIdAsync(barbershopId, cancellationToken);
            _logger.LogInformation("Landing page config retrieved successfully for barbershop: {BarbershopId}", barbershopId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Landing page not found for barbershop: {BarbershopId}. Error: {Error}", 
                barbershopId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza a configuração da landing page
    /// </summary>
    /// <param name="barbershopId">ID da barbearia</param>
    /// <param name="input">Dados de atualização</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status da operação</returns>
    /// <response code="204">Configuração atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Token inválido ou expirado</response>
    /// <response code="403">Admin não pertence à barbearia solicitada</response>
    /// <response code="404">Landing page não encontrada</response>
    [HttpPut("{barbershopId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConfig(
        [FromRoute] Guid barbershopId,
        [FromBody] UpdateLandingPageInput input,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating landing page config for barbershop: {BarbershopId}", barbershopId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for landing page update. Barbershop: {BarbershopId}", barbershopId);
            return BadRequest(ModelState);
        }

        if (!IsAuthorizedForBarbershop(barbershopId))
        {
            _logger.LogWarning("Admin unauthorized to update barbershop: {BarbershopId}", barbershopId);
            return Forbid();
        }

        try
        {
            await _landingPageService.UpdateConfigAsync(barbershopId, input, cancellationToken);
            _logger.LogInformation("Landing page config updated successfully for barbershop: {BarbershopId}", barbershopId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Landing page not found for barbershop: {BarbershopId}. Error: {Error}", 
                barbershopId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation for barbershop: {BarbershopId}. Error: {Error}", 
                barbershopId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument for barbershop: {BarbershopId}. Error: {Error}", 
                barbershopId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Faz upload do logo da landing page
    /// </summary>
    /// <param name="barbershopId">ID da barbearia</param>
    /// <param name="file">Arquivo de imagem (JPG, PNG ou SVG, máx. 2MB)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>URL do logo</returns>
    /// <response code="200">Logo enviado com sucesso</response>
    /// <response code="400">Arquivo inválido</response>
    /// <response code="401">Token inválido ou expirado</response>
    /// <response code="403">Admin não pertence à barbearia solicitada</response>
    /// <response code="404">Landing page não encontrada</response>
    [HttpPost("{barbershopId:guid}/logo")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadLogo(
        [FromRoute] Guid barbershopId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading logo for barbershop: {BarbershopId}", barbershopId);

        if (!IsAuthorizedForBarbershop(barbershopId))
        {
            _logger.LogWarning("Admin unauthorized to upload logo for barbershop: {BarbershopId}", barbershopId);
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "Nenhum arquivo foi enviado" });
        }

        // Usar o serviço de upload de logo
        var uploadResult = await _logoUploadService.UploadLogoAsync(barbershopId, file, cancellationToken);

        if (!uploadResult.IsSuccess)
        {
            _logger.LogWarning("Logo upload failed for barbershop: {BarbershopId}. Error: {Error}",
                barbershopId, uploadResult.Error);
            return BadRequest(new { error = uploadResult.Error });
        }

        // Atualizar landing page com nova URL do logo
        var updateInput = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: uploadResult.Data,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        try
        {
            await _landingPageService.UpdateConfigAsync(barbershopId, updateInput, cancellationToken);
            _logger.LogInformation("Logo uploaded successfully for barbershop: {BarbershopId}", barbershopId);
            
            return Ok(new
            {
                logoUrl = uploadResult.Data,
                message = "Logo atualizado com sucesso"
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Landing page not found for barbershop: {BarbershopId}. Error: {Error}", 
                barbershopId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload logo for barbershop: {BarbershopId}", barbershopId);
            return BadRequest(new { error = "Erro ao fazer upload do logo" });
        }
    }

    private bool IsAuthorizedForBarbershop(Guid barbershopId)
    {
        var userType = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // AdminCentral can access any barbershop
        if (userType == "AdminCentral")
        {
            return true;
        }
        
        // AdminBarbearia can only access their own barbershop
        var userBarbershopId = User.FindFirst("barbeariaId")?.Value;
        if (string.IsNullOrEmpty(userBarbershopId))
        {
            return false;
        }

        return Guid.TryParse(userBarbershopId, out var parsedId) && parsedId == barbershopId;
    }
}
