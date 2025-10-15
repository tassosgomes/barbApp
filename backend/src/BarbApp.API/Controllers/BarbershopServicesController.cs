using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/barbershop-services")]
[Authorize(Roles = "AdminBarbearia")]
[Produces("application/json")]
public class BarbershopServicesController : ControllerBase
{
    private readonly ICreateBarbershopServiceUseCase _createServiceUseCase;
    private readonly IUpdateBarbershopServiceUseCase _updateServiceUseCase;
    private readonly IDeleteBarbershopServiceUseCase _deleteServiceUseCase;
    private readonly IListBarbershopServicesUseCase _listServicesUseCase;
    private readonly IGetBarbershopServiceByIdUseCase _getServiceByIdUseCase;
    private readonly ILogger<BarbershopServicesController> _logger;

    public BarbershopServicesController(
        ICreateBarbershopServiceUseCase createServiceUseCase,
        IUpdateBarbershopServiceUseCase updateServiceUseCase,
        IDeleteBarbershopServiceUseCase deleteServiceUseCase,
        IListBarbershopServicesUseCase listServicesUseCase,
        IGetBarbershopServiceByIdUseCase getServiceByIdUseCase,
        ILogger<BarbershopServicesController> logger)
    {
        _createServiceUseCase = createServiceUseCase;
        _updateServiceUseCase = updateServiceUseCase;
        _deleteServiceUseCase = deleteServiceUseCase;
        _listServicesUseCase = listServicesUseCase;
        _getServiceByIdUseCase = getServiceByIdUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo serviço oferecido pela barbearia
    /// </summary>
    /// <param name="input">Dados do serviço a ser criado</param>
    /// <returns>Dados do serviço criado</returns>
    /// <response code="201">Serviço criado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="409">Nome do serviço já existe nesta barbearia</response>
    /// <response code="422">Erro de validação de negócio</response>
    [HttpPost]
    [ProducesResponseType(typeof(BarbershopServiceOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BarbershopServiceOutput>> CreateService([FromBody] CreateBarbershopServiceInput input)
    {
        _logger.LogInformation("Creating new service: {Name}", input.Name);

        var result = await _createServiceUseCase.ExecuteAsync(input, HttpContext.RequestAborted);

        _logger.LogInformation("Service created successfully with ID: {Id}", result.Id);

        return CreatedAtAction(nameof(GetServiceById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista serviços oferecidos pela barbearia com paginação
    /// </summary>
    /// <param name="isActive">Filtrar por status ativo/inativo</param>
    /// <param name="searchName">Buscar por nome</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100)</param>
    /// <returns>Lista paginada de serviços</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="400">Parâmetros de paginação inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedBarbershopServicesOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedBarbershopServicesOutput>> ListServices(
        [FromQuery] bool? isActive = true,
        [FromQuery] string? searchName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Listing services with filters - isActive: {IsActive}, searchName: {SearchName}, page: {Page}, pageSize: {PageSize}",
            isActive, searchName, page, pageSize);

        var result = await _listServicesUseCase.ExecuteAsync(isActive, searchName, page, pageSize, HttpContext.RequestAborted);

        _logger.LogInformation("Returned {Count} services", result.Services.Count);

        return Ok(result);
    }

    /// <summary>
    /// Obtém detalhes de um serviço específico
    /// </summary>
    /// <param name="id">ID do serviço</param>
    /// <returns>Dados do serviço</returns>
    /// <response code="200">Serviço encontrado</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="404">Serviço não encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BarbershopServiceOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarbershopServiceOutput>> GetServiceById(Guid id)
    {
        _logger.LogInformation("Getting service by ID: {Id}", id);

        var result = await _getServiceByIdUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Service found: {Name} ({Id})", result.Name, result.Id);

        return Ok(result);
    }

    /// <summary>
    /// Atualiza informações de um serviço
    /// </summary>
    /// <param name="id">ID do serviço</param>
    /// <param name="input">Dados atualizados do serviço</param>
    /// <returns>Dados do serviço atualizado</returns>
    /// <response code="200">Serviço atualizado com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="404">Serviço não encontrado</response>
    /// <response code="409">Nome do serviço já existe nesta barbearia</response>
    /// <response code="422">Erro de validação de negócio</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BarbershopServiceOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BarbershopServiceOutput>> UpdateService(Guid id, [FromBody] UpdateBarbershopServiceInput input)
    {
        _logger.LogInformation("Updating service {Id}", id);

        var updateInput = input with { Id = id };
        var result = await _updateServiceUseCase.ExecuteAsync(updateInput, HttpContext.RequestAborted);

        _logger.LogInformation("Service updated successfully: {Name} ({Id})", result.Name, result.Id);

        return Ok(result);
    }

    /// <summary>
    /// Remove um serviço oferecido pela barbearia
    /// </summary>
    /// <param name="id">ID do serviço</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Serviço removido com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para acessar este recurso</response>
    /// <response code="404">Serviço não encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        _logger.LogInformation("Deleting service {Id}", id);

        await _deleteServiceUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Service deleted successfully: {Id}", id);

        return NoContent();
    }
}