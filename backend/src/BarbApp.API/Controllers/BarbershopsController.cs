using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/barbearias")]
[Authorize]
[Produces("application/json")]
public class BarbershopsController : ControllerBase
{
    private readonly ICreateBarbershopUseCase _createBarbershopUseCase;
    private readonly IUpdateBarbershopUseCase _updateBarbershopUseCase;
    private readonly IDeleteBarbershopUseCase _deleteBarbershopUseCase;
    private readonly IDeactivateBarbershopUseCase _deactivateBarbershopUseCase;
    private readonly IReactivateBarbershopUseCase _reactivateBarbershopUseCase;
    private readonly IGetBarbershopUseCase _getBarbershopUseCase;
    private readonly IListBarbershopsUseCase _listBarbershopsUseCase;
    private readonly IResendCredentialsUseCase _resendCredentialsUseCase;
    private readonly IGetMyBarbershopUseCase _getMyBarbershopUseCase;
    private readonly ILogger<BarbershopsController> _logger;

    public BarbershopsController(
        ICreateBarbershopUseCase createBarbershopUseCase,
        IUpdateBarbershopUseCase updateBarbershopUseCase,
        IDeleteBarbershopUseCase deleteBarbershopUseCase,
        IDeactivateBarbershopUseCase deactivateBarbershopUseCase,
        IReactivateBarbershopUseCase reactivateBarbershopUseCase,
        IGetBarbershopUseCase getBarbershopUseCase,
        IListBarbershopsUseCase listBarbershopsUseCase,
        IResendCredentialsUseCase resendCredentialsUseCase,
        IGetMyBarbershopUseCase getMyBarbershopUseCase,
        ILogger<BarbershopsController> logger)
    {
        _createBarbershopUseCase = createBarbershopUseCase;
        _updateBarbershopUseCase = updateBarbershopUseCase;
        _deleteBarbershopUseCase = deleteBarbershopUseCase;
        _deactivateBarbershopUseCase = deactivateBarbershopUseCase;
        _reactivateBarbershopUseCase = reactivateBarbershopUseCase;
        _getBarbershopUseCase = getBarbershopUseCase;
        _listBarbershopsUseCase = listBarbershopsUseCase;
        _resendCredentialsUseCase = resendCredentialsUseCase;
        _getMyBarbershopUseCase = getMyBarbershopUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Valida o código de uma barbearia (endpoint público para tela de login)
    /// </summary>
    /// <param name="codigo">Código da barbearia (8 caracteres alfanuméricos)</param>
    /// <param name="useCase">Use case injetado via DI</param>
    /// <returns>Informações básicas da barbearia</returns>
    /// <response code="200">Código válido - retorna dados básicos da barbearia</response>
    /// <response code="400">Código com formato inválido</response>
    /// <response code="403">Barbearia inativa</response>
    /// <response code="404">Código não encontrado</response>
    [HttpGet("codigo/{codigo}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidateBarbeariaCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ValidateBarbeariaCodeResponse>> GetByCode(
        [FromRoute] string codigo,
        [FromServices] ValidateBarbeariaCodeUseCase useCase)
    {
        _logger.LogInformation("Validating barbershop code: {Code}", codigo);

        var result = await useCase.ExecuteAsync(codigo, HttpContext.RequestAborted);

        _logger.LogInformation("Code validated successfully for barbershop: {Name} (ID: {Id})", result.Nome, result.Id);

        return Ok(result);
    }

    /// <summary>
    /// Obtém os dados completos da barbearia do usuário autenticado (Admin Barbearia)
    /// </summary>
    /// <returns>Dados completos da barbearia</returns>
    /// <response code="200">Dados da barbearia retornados com sucesso</response>
    /// <response code="401">Token inválido ou expirado</response>
    /// <response code="403">Usuário não é Admin Barbearia ou não tem barbearia associada</response>
    /// <response code="404">Barbearia não encontrada</response>
    [HttpGet("me")]
    [Authorize(Roles = "AdminBarbearia")]
    [ProducesResponseType(typeof(BarbershopOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarbershopOutput>> GetMyBarbershop()
    {
        _logger.LogInformation("Getting my barbershop data");

        var result = await _getMyBarbershopUseCase.ExecuteAsync(HttpContext.RequestAborted);

        _logger.LogInformation("My barbershop data retrieved successfully for barbershop: {Name} (ID: {Id})", result.Name, result.Id);

        return Ok(result);
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
    [Authorize(Roles = "AdminCentral")]
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
    [Authorize(Roles = "AdminCentral")]
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
    [Authorize(Roles = "AdminCentral")]
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
    [Authorize(Roles = "AdminCentral")]
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
    [Authorize(Roles = "AdminCentral")]
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

    /// <summary>
    /// Desativa uma barbearia (soft delete)
    /// </summary>
    /// <param name="id">ID da barbearia a ser desativada</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Barbearia desativada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para desativar barbearias</response>
    /// <response code="404">Barbearia não encontrada</response>
    [HttpPut("{id:guid}/desativar")]
    [Authorize(Roles = "AdminCentral")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateBarbershop(Guid id)
    {
        _logger.LogInformation("Deactivating barbershop with ID: {Id}", id);

        await _deactivateBarbershopUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Barbershop deactivated successfully with ID: {Id}", id);

        return NoContent();
    }

    /// <summary>
    /// Reativa uma barbearia
    /// </summary>
    /// <param name="id">ID da barbearia a ser reativada</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Barbearia reativada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para reativar barbearias</response>
    /// <response code="404">Barbearia não encontrada</response>
    [HttpPut("{id:guid}/reativar")]
    [Authorize(Roles = "AdminCentral")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateBarbershop(Guid id)
    {
        _logger.LogInformation("Reactivating barbershop with ID: {Id}", id);

        await _reactivateBarbershopUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Barbershop reactivated successfully with ID: {Id}", id);

        return NoContent();
    }

    /// <summary>
    /// Reenvia as credenciais de acesso do Admin Barbearia
    /// </summary>
    /// <param name="id">ID da barbearia</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Credenciais reenviadas com sucesso</response>
    /// <response code="400">Administrador da barbearia não encontrado</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão para reenviar credenciais</response>
    /// <response code="404">Barbearia não encontrada</response>
    /// <response code="500">Falha ao enviar e-mail</response>
    [HttpPost("{id:guid}/reenviar-credenciais")]
    [Authorize(Roles = "AdminCentral")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendCredentials(Guid id)
    {
        _logger.LogInformation("Resending credentials for barbershop with ID: {Id}", id);

        await _resendCredentialsUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Credentials resent successfully for barbershop with ID: {Id}", id);

        return Ok(new { message = "Credenciais reenviadas com sucesso. O administrador receberá um e-mail com a nova senha." });
    }
}