using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize(Roles = "Barbeiro")]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IGetAppointmentDetailsUseCase _getAppointmentDetailsUseCase;
    private readonly IConfirmAppointmentUseCase _confirmAppointmentUseCase;
    private readonly ICancelAppointmentUseCase _cancelAppointmentUseCase;
    private readonly ICompleteAppointmentUseCase _completeAppointmentUseCase;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IGetAppointmentDetailsUseCase getAppointmentDetailsUseCase,
        IConfirmAppointmentUseCase confirmAppointmentUseCase,
        ICancelAppointmentUseCase cancelAppointmentUseCase,
        ICompleteAppointmentUseCase completeAppointmentUseCase,
        ILogger<AppointmentsController> logger)
    {
        _getAppointmentDetailsUseCase = getAppointmentDetailsUseCase;
        _confirmAppointmentUseCase = confirmAppointmentUseCase;
        _cancelAppointmentUseCase = cancelAppointmentUseCase;
        _completeAppointmentUseCase = completeAppointmentUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Obtém detalhes de um agendamento específico
    /// </summary>
    /// <param name="id">ID do agendamento</param>
    /// <returns>Detalhes completos do agendamento</returns>
    /// <response code="200">Agendamento encontrado</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Agendamento não pertence ao barbeiro</response>
    /// <response code="404">Agendamento não encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppointmentDetailsOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDetailsOutput>> GetAppointmentDetails(Guid id)
    {
        _logger.LogInformation("Getting appointment details for ID: {Id}", id);

        var result = await _getAppointmentDetailsUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Appointment details retrieved: {Id}", result.Id);

        return Ok(result);
    }

    /// <summary>
    /// Confirma um agendamento pendente
    /// </summary>
    /// <param name="id">ID do agendamento</param>
    /// <returns>Detalhes do agendamento confirmado</returns>
    /// <response code="200">Agendamento confirmado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Agendamento não pertence ao barbeiro</response>
    /// <response code="404">Agendamento não encontrado</response>
    /// <response code="409">Status do agendamento não permite confirmação</response>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(typeof(AppointmentDetailsOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDetailsOutput>> ConfirmAppointment(Guid id)
    {
        _logger.LogInformation("Confirming appointment: {Id}", id);

        var result = await _confirmAppointmentUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Appointment confirmed successfully: {Id}", id);

        return Ok(result);
    }

    /// <summary>
    /// Cancela um agendamento (pendente ou confirmado)
    /// </summary>
    /// <param name="id">ID do agendamento</param>
    /// <returns>Detalhes do agendamento cancelado</returns>
    /// <response code="200">Agendamento cancelado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Agendamento não pertence ao barbeiro</response>
    /// <response code="404">Agendamento não encontrado</response>
    /// <response code="409">Status do agendamento não permite cancelamento</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(AppointmentDetailsOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDetailsOutput>> CancelAppointment(Guid id)
    {
        _logger.LogInformation("Cancelling appointment: {Id}", id);

        var result = await _cancelAppointmentUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Appointment cancelled successfully: {Id}", id);

        return Ok(result);
    }

    /// <summary>
    /// Marca um agendamento confirmado como concluído
    /// </summary>
    /// <param name="id">ID do agendamento</param>
    /// <returns>Detalhes do agendamento concluído</returns>
    /// <response code="200">Agendamento concluído com sucesso</response>
    /// <response code="400">Agendamento ainda não começou ou não está confirmado</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Agendamento não pertence ao barbeiro</response>
    /// <response code="404">Agendamento não encontrado</response>
    /// <response code="409">Status do agendamento não permite conclusão</response>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(AppointmentDetailsOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDetailsOutput>> CompleteAppointment(Guid id)
    {
        _logger.LogInformation("Completing appointment: {Id}", id);

        var result = await _completeAppointmentUseCase.ExecuteAsync(id, HttpContext.RequestAborted);

        _logger.LogInformation("Appointment completed successfully: {Id}", id);

        return Ok(result);
    }
}
