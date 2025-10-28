using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/barbeiros")]
[Authorize(Roles = "Cliente")]
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
        var barbeariaId = _tenantContext.BarbeariaId ?? throw new UnauthorizedAccessException("Contexto de barbearia não encontrado");
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
    /// <param name="cancellationToken">Token de cancelamento</param>
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
        var barbeariaId = _tenantContext.BarbeariaId ?? throw new UnauthorizedAccessException("Contexto de barbearia não encontrado");
        
        // Verificar se o barbeiro pertence à barbearia do cliente
        var barbeiro = await _listarBarbeirosUseCase.Handle(barbeariaId, cancellationToken);
        if (!barbeiro.Any(b => b.Id == barbeiroId))
        {
            return Forbid();
        }

        var disponibilidade = await _consultarDisponibilidadeUseCase.Handle(
            barbeiroId,
            dataInicio,
            dataFim,
            duracaoMinutos,
            cancellationToken);

        return Ok(disponibilidade);
    }
}