using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarbApp.API.Controllers;

[ApiController]
[Route("api/schedule")]
[Authorize(Roles = "Barbeiro")]
[Produces("application/json")]
public class ScheduleController : ControllerBase
{
    private readonly IGetBarberScheduleUseCase _getBarberScheduleUseCase;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(
        IGetBarberScheduleUseCase getBarberScheduleUseCase,
        ILogger<ScheduleController> logger)
    {
        _getBarberScheduleUseCase = getBarberScheduleUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Obtém a agenda do barbeiro autenticado para uma data específica
    /// </summary>
    /// <param name="date">Data para consultar a agenda (formato: YYYY-MM-DD). Se não informado, utiliza a data atual.</param>
    /// <returns>Agenda do barbeiro com lista de agendamentos</returns>
    /// <response code="200">Agenda retornada com sucesso</response>
    /// <response code="400">Data inválida</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem permissão de barbeiro</response>
    /// <response code="404">Barbeiro não encontrado</response>
    [HttpGet("my-schedule")]
    [ProducesResponseType(typeof(BarberScheduleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarberScheduleOutput>> GetMySchedule([FromQuery] DateTime? date = null)
    {
        var scheduleDate = date ?? DateTime.UtcNow.Date;
        
        _logger.LogInformation("Getting schedule for barber on date: {Date}", scheduleDate);

        var result = await _getBarberScheduleUseCase.ExecuteAsync(scheduleDate, HttpContext.RequestAborted);

        _logger.LogInformation("Schedule returned with {AppointmentCount} appointments", result.Appointments.Count);

        return Ok(result);
    }
}
