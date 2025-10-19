using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/barbers")]
[Authorize(Roles = "AdminBarbearia")]
[Produces("application/json")]
public class BarbersController : ControllerBase
{
    private readonly ICreateBarberUseCase _createBarberUseCase;
    private readonly IUpdateBarberUseCase _updateBarberUseCase;
    private readonly IRemoveBarberUseCase _removeBarberUseCase;
    private readonly IListBarbersUseCase _listBarbersUseCase;
    private readonly IGetBarberByIdUseCase _getBarberByIdUseCase;
    private readonly IGetTeamScheduleUseCase _getTeamScheduleUseCase;
    private readonly ILogger<BarbersController> _logger;

    public BarbersController(
        ICreateBarberUseCase createBarberUseCase,
        IUpdateBarberUseCase updateBarberUseCase,
        IRemoveBarberUseCase removeBarberUseCase,
        IListBarbersUseCase listBarbersUseCase,
        IGetBarberByIdUseCase getBarberByIdUseCase,
        IGetTeamScheduleUseCase getTeamScheduleUseCase,
        ILogger<BarbersController> logger)
    {
        _createBarberUseCase = createBarberUseCase;
        _updateBarberUseCase = updateBarberUseCase;
        _removeBarberUseCase = removeBarberUseCase;
        _listBarbersUseCase = listBarbersUseCase;
        _getBarberByIdUseCase = getBarberByIdUseCase;
        _getTeamScheduleUseCase = getTeamScheduleUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo barbeiro na barbearia
    /// </summary>
    /// <param name="input">Dados do barbeiro a ser criado</param>
    /// <returns>Dados do barbeiro criado</returns>
    /// <response code="201">Barbeiro criado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="409">Email já cadastrado nesta barbearia</response>
    /// <response code="422">Erro de validação de negócio</response>
    [HttpPost]
    [ProducesResponseType(typeof(BarberOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BarberOutput>> CreateBarber([FromBody] CreateBarberInput input)
    {
        _logger.LogInformation("Creating new barber with email: {Email}", input.Email);

        var result = await _createBarberUseCase.ExecuteAsync(input, HttpContext.RequestAborted);

        _logger.LogInformation("Barber created successfully with ID: {Id}", result.Id);

        return CreatedAtAction(nameof(GetBarberById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista barbeiros da barbearia com paginação
    /// </summary>
    /// <param name="isActive">Filtrar por status ativo/inativo</param>
    /// <param name="searchName">Buscar por nome</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100)</param>
    /// <returns>Lista paginada de barbeiros</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="400">Parâmetros de paginação inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedBarbersOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedBarbersOutput>> ListBarbers(
        [FromQuery] bool? isActive = true,
        [FromQuery] string? searchName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Listing barbers with filters - isActive: {IsActive}, searchName: {SearchName}, page: {Page}, pageSize: {PageSize}",
            isActive, searchName, page, pageSize);

        var result = await _listBarbersUseCase.ExecuteAsync(isActive, searchName, page, pageSize, HttpContext.RequestAborted);

        _logger.LogInformation("Returned {Count} barbers", result.Barbers.Count);

        return Ok(result);
    }

    /// <summary>
    /// Obtém detalhes de um barbeiro específico
    /// </summary>
    /// <param name="id">ID do barbeiro</param>
    /// <returns>Dados do barbeiro</returns>
    /// <response code="200">Barbeiro encontrado</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="404">Barbeiro não encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BarberOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarberOutput>> GetBarberById(Guid id)
    {
        _logger.LogInformation("Getting barber by ID: {Id}", id);

        var result = await _getBarberByIdUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Barber found: {Name} ({Id})", result.Name, result.Id);

        return Ok(result);
    }

    /// <summary>
    /// Atualiza informações de um barbeiro
    /// </summary>
    /// <param name="id">ID do barbeiro</param>
    /// <param name="input">Dados atualizados do barbeiro</param>
    /// <returns>Dados do barbeiro atualizado</returns>
    /// <response code="200">Barbeiro atualizado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="404">Barbeiro não encontrado</response>
    /// <response code="409">Email já cadastrado nesta barbearia</response>
    /// <response code="422">Erro de validação de negócio</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BarberOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BarberOutput>> UpdateBarber(Guid id, [FromBody] UpdateBarberInput input)
    {
        _logger.LogInformation("Updating barber {Id}", id);

        var updateInput = input with { Id = id };
        var result = await _updateBarberUseCase.ExecuteAsync(updateInput, HttpContext.RequestAborted);

        _logger.LogInformation("Barber updated successfully: {Name} ({Id})", result.Name, result.Id);

        return Ok(result);
    }

    /// <summary>
    /// Desativa um barbeiro da barbearia (não poderá receber novos agendamentos)
    /// </summary>
    /// <param name="id">ID do barbeiro</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Barbeiro desativado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="404">Barbeiro não encontrado</response>
    [HttpPut("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateBarber(Guid id)
    {
        _logger.LogInformation("Deactivating barber {Id}", id);

        await _removeBarberUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Barber deactivated successfully: {Id}", id);

        return NoContent();
    }

    /// <summary>
    /// Remove um barbeiro da barbearia (desativa e cancela agendamentos futuros)
    /// </summary>
    /// <param name="id">ID do barbeiro</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Barbeiro removido com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="404">Barbeiro não encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveBarber(Guid id)
    {
        _logger.LogInformation("Removing barber {Id}", id);

        await _removeBarberUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Barber removed successfully: {Id}", id);

        return NoContent();
    }

    /// <summary>
    /// Obtém agenda consolidada de todos os barbeiros da barbearia
    /// </summary>
    /// <param name="date">Data para filtrar a agenda (formato: YYYY-MM-DD)</param>
    /// <param name="barberId">ID do barbeiro para filtrar (opcional)</param>
    /// <returns>Agenda da equipe</returns>
    /// <response code="200">Agenda retornada com sucesso</response>
    /// <response code="400">Data inválida</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    [HttpGet("schedule")]
    [ProducesResponseType(typeof(TeamScheduleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamScheduleOutput>> GetTeamSchedule([FromQuery] DateTime? date = null, [FromQuery] Guid? barberId = null)
    {
        var scheduleDate = date ?? DateTime.UtcNow.Date;
        _logger.LogInformation("Getting team schedule for date: {Date}, barberId: {BarberId}", scheduleDate, barberId);

        var result = await _getTeamScheduleUseCase.ExecuteAsync(scheduleDate, barberId, HttpContext.RequestAborted);

        _logger.LogInformation("Team schedule returned with {AppointmentCount} appointments", result.Appointments.Count);

        return Ok(result);
    }
}