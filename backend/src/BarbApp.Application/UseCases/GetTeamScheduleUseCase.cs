using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class GetTeamScheduleUseCase : IGetTeamScheduleUseCase
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTeamScheduleUseCase> _logger;

    public GetTeamScheduleUseCase(
        ITenantContext tenantContext,
        ILogger<GetTeamScheduleUseCase> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<TeamScheduleOutput> ExecuteAsync(DateTime date, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting team schedule for date {Date}", date);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // TODO: Implement when Appointment entity is fully implemented
        // For now, return empty schedule
        var appointments = new List<AppointmentOutput>();

        _logger.LogInformation("Team schedule retrieved for date {Date} with {Count} appointments", date, appointments.Count);

        return new TeamScheduleOutput(appointments);
    }
}