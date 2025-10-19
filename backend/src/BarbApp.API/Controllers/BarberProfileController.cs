using BarbApp.Application.DTOs;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarbApp.API.Controllers;

/// <summary>
/// Controller para gerenciar o perfil do barbeiro autenticado
/// </summary>
[ApiController]
[Route("api/barber")]
[Authorize(Roles = "Barbeiro")]
[Produces("application/json")]
public class BarberProfileController : ControllerBase
{
    private readonly IBarberRepository _barberRepository;
    private readonly ILogger<BarberProfileController> _logger;

    public BarberProfileController(
        IBarberRepository barberRepository,
        ILogger<BarberProfileController> logger)
    {
        _barberRepository = barberRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o perfil do barbeiro autenticado
    /// </summary>
    /// <returns>Dados do perfil do barbeiro</returns>
    /// <response code="200">Perfil retornado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="404">Barbeiro não encontrado</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(BarberProfileOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarberProfileOutput>> GetProfile()
    {
        // Pega o ID do barbeiro do token JWT
        var barberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(barberIdClaim) || !Guid.TryParse(barberIdClaim, out var barberId))
        {
            _logger.LogWarning("Invalid or missing NameIdentifier claim in token");
            return Unauthorized(new { message = "Token inválido" });
        }

        _logger.LogInformation("Getting profile for barber ID: {BarberId}", barberId);

        var barber = await _barberRepository.GetByIdAsync(barberId, HttpContext.RequestAborted);

        if (barber == null)
        {
            _logger.LogWarning("Barber not found: {BarberId}", barberId);
            return NotFound(new { message = "Barbeiro não encontrado" });
        }

        var response = new BarberProfileOutput
        {
            Id = barber.Id,
            Name = barber.Name,
            Email = barber.Email,
            PhoneNumber = barber.Phone,
            IsActive = barber.IsActive,
            BarbeariaId = barber.BarbeariaId,
            BarbeariaNome = barber.Barbearia?.Name ?? string.Empty,
            CreatedAt = barber.CreatedAt
        };

        _logger.LogInformation("Profile returned successfully for barber: {Name} ({BarberId})", barber.Name, barberId);

        return Ok(response);
    }
}
