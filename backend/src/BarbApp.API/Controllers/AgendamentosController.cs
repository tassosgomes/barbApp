using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/agendamentos")]
[Authorize(Roles = "Cliente")]
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
        var clienteId = Guid.Parse(_tenantContext.UserId);
        var barbeariaId = _tenantContext.BarbeariaId ?? throw new UnauthorizedAccessException("Contexto de barbearia não encontrado");

        _logger.LogInformation("Cliente {ClienteId} criando agendamento na barbearia {BarbeariaId}",
            clienteId, barbeariaId);

        var agendamento = await _criarAgendamentoUseCase.Handle(clienteId, barbeariaId, input, cancellationToken);

        return CreatedAtAction(nameof(CriarAgendamento), agendamento);
    }

    /// <summary>
    /// Listar agendamentos do cliente autenticado
    /// </summary>
    /// <param name="filtro">Filtro: "proximos" ou "historico"</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [HttpGet("meus")]
    [ProducesResponseType(typeof(List<AgendamentoOutput>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AgendamentoOutput>>> MeusAgendamentos(
        [FromQuery] string filtro = "proximos",
        CancellationToken cancellationToken = default)
    {
        var clienteId = Guid.Parse(_tenantContext.UserId);
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
        var clienteId = Guid.Parse(_tenantContext.UserId);

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
        var clienteId = Guid.Parse(_tenantContext.UserId);
        var barbeariaId = _tenantContext.BarbeariaId ?? throw new UnauthorizedAccessException("Contexto de barbearia não encontrado");

        var agendamento = await _editarAgendamentoUseCase.Handle(
            clienteId,
            barbeariaId,
            new EditarAgendamentoInput(agendamentoId, input.BarbeiroId, input.ServicosIds, input.DataHora),
            cancellationToken);

        return Ok(agendamento);
    }
}