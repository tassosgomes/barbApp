using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/servicos")]
[Authorize(Roles = "Cliente")]
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
        var barbeariaId = _tenantContext.BarbeariaId ?? throw new UnauthorizedAccessException("Contexto de barbearia não encontrado");
        var servicos = await _listarServicosUseCase.Handle(barbeariaId, cancellationToken);
        return Ok(servicos);
    }
}