using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/barbearias")]
[Authorize(Roles = "AdminCentral")]
[Produces("application/json")]
public class BarbershopsController : ControllerBase
{
    private readonly ICreateBarbershopUseCase _createBarbershopUseCase;
    private readonly IUpdateBarbershopUseCase _updateBarbershopUseCase;
    private readonly IDeleteBarbershopUseCase _deleteBarbershopUseCase;
    private readonly IGetBarbershopUseCase _getBarbershopUseCase;
    private readonly IListBarbershopsUseCase _listBarbershopsUseCase;
    private readonly ILogger<BarbershopsController> _logger;

    public BarbershopsController(
        ICreateBarbershopUseCase createBarbershopUseCase,
        IUpdateBarbershopUseCase updateBarbershopUseCase,
        IDeleteBarbershopUseCase deleteBarbershopUseCase,
        IGetBarbershopUseCase getBarbershopUseCase,
        IListBarbershopsUseCase listBarbershopsUseCase,
        ILogger<BarbershopsController> logger)
    {
        _createBarbershopUseCase = createBarbershopUseCase;
        _updateBarbershopUseCase = updateBarbershopUseCase;
        _deleteBarbershopUseCase = deleteBarbershopUseCase;
        _getBarbershopUseCase = getBarbershopUseCase;
        _listBarbershopsUseCase = listBarbershopsUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova barbearia no sistema
    /// </summary>
    /// <param name="input">Dados da barbearia a ser criada</param>
    /// <returns>Informações da barbearia criada incluindo o código único gerado</returns>
    /// <response code="201">Barbearia criada com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para criar barbearias</response>
    /// <response code="422">Documento já cadastrado ou outros erros de negócio</response>
    [HttpPost]
    [ProducesResponseType(typeof(BarbershopOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BarbershopOutput>> CreateBarbershop([FromBody] CreateBarbershopInput input)
    {
        _logger.LogInformation("Creating new barbershop: {Name}", input.Name);

        var result = await _createBarbershopUseCase.ExecuteAsync(input, HttpContext.RequestAborted);

        _logger.LogInformation("Barbershop created successfully with ID: {Id} and code: {Code}", result.Id, result.Code);

        return CreatedAtAction(nameof(GetBarbershop), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza os dados de uma barbearia existente
    /// </summary>
    /// <param name="id">ID da barbearia a ser atualizada</param>
    /// <param name="input">Novos dados da barbearia</param>
    /// <returns>Informações atualizadas da barbearia</returns>
    /// <response code="200">Barbearia atualizada com sucesso</response>
    /// <response code="400">Dados de entrada inválidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para atualizar barbearias</response>
    /// <response code="404">Barbearia não encontrada</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BarbershopOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarbershopOutput>> UpdateBarbershop(Guid id, [FromBody] UpdateBarbershopInput input)
    {
        if (id != input.Id)
        {
            return BadRequest("ID mismatch");
        }

        _logger.LogInformation("Updating barbershop with ID: {Id}", id);

        var result = await _updateBarbershopUseCase.ExecuteAsync(input, HttpContext.RequestAborted);

        _logger.LogInformation("Barbershop updated successfully with ID: {Id}", id);

        return Ok(result);
    }

    /// <summary>
    /// Obtém os dados de uma barbearia específica
    /// </summary>
    /// <param name="id">ID da barbearia</param>
    /// <returns>Informações da barbearia</returns>
    /// <response code="200">Barbearia encontrada</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para visualizar barbearias</response>
    /// <response code="404">Barbearia não encontrada</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BarbershopOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarbershopOutput>> GetBarbershop(Guid id)
    {
        _logger.LogInformation("Getting barbershop with ID: {Id}", id);

        var result = await _getBarbershopUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        return Ok(result);
    }

    /// <summary>
    /// Lista barbearias com paginação e filtros
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100)</param>
    /// <param name="searchTerm">Termo de busca (nome, código ou documento)</param>
    /// <param name="isActive">Filtrar por status ativo/inativo</param>
    /// <param name="sortBy">Campo para ordenação (name, createdAt)</param>
    /// <returns>Lista paginada de barbearias</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para listar barbearias</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedBarbershopsOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedBarbershopsOutput>> ListBarbershops(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null)
    {
        // Validate page parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        _logger.LogInformation(
            "Listing barbershops - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, IsActive: {IsActive}, SortBy: {SortBy}",
            page, pageSize, searchTerm, isActive, sortBy);

        var result = await _listBarbershopsUseCase.ExecuteAsync(
            page,
            pageSize,
            searchTerm,
            isActive,
            sortBy,
            HttpContext.RequestAborted);

        return Ok(result);
    }

    /// <summary>
    /// Exclui uma barbearia do sistema
    /// </summary>
    /// <param name="id">ID da barbearia a ser excluída</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Barbearia excluída com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para excluir barbearias</response>
    /// <response code="404">Barbearia não encontrada</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBarbershop(Guid id)
    {
        _logger.LogInformation("Deleting barbershop with ID: {Id}", id);

        await _deleteBarbershopUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Barbershop deleted successfully with ID: {Id}", id);

        return NoContent();
    }
}