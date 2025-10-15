using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class RemoveBarberUseCase : IRemoveBarberUseCase
{
    private readonly IBarberRepository _barberRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveBarberUseCase> _logger;

    public RemoveBarberUseCase(
        IBarberRepository barberRepository,
        IAppointmentRepository appointmentRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<RemoveBarberUseCase> logger)
    {
        _barberRepository = barberRepository;
        _appointmentRepository = appointmentRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid barberId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting removal of barber with ID {BarberId}", barberId);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Get existing barber
        var barber = await _barberRepository.GetByIdAsync(barberId, cancellationToken);
        if (barber == null || barber.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning("Barber with ID {BarberId} not found or does not belong to barbearia {BarbeariaId}", barberId, barbeariaId);
            throw new BarberNotFoundException(barberId);
        }

        // Get future appointments for this barber
        var futureAppointments = await _appointmentRepository.GetFutureAppointmentsByBarberAsync(barberId, cancellationToken);
        _logger.LogInformation("Found {Count} future appointments for barber {BarberId}", futureAppointments.Count, barberId);

        if (futureAppointments.Any())
        {
            // Cancel all future appointments
            await _appointmentRepository.UpdateStatusAsync(futureAppointments, "Cancelled", cancellationToken);
            _logger.LogInformation("Cancelled {Count} future appointments for barber {BarberId}", futureAppointments.Count, barberId);
        }

        // Deactivate the barber (soft delete)
        barber.Deactivate();
        await _barberRepository.UpdateAsync(barber, cancellationToken);

        // Commit all changes atomically
        await _unitOfWork.Commit(cancellationToken);

        // Increment metrics
        BarbAppMetrics.BarberRemovedCounter.WithLabels(barbeariaId.Value.ToString()).Inc();
        BarbAppMetrics.ActiveBarbersGauge.WithLabels(barbeariaId.Value.ToString()).Dec();

        _logger.LogInformation("Barber {BarberId} removed successfully (deactivated) and {AppointmentCount} future appointments cancelled", barberId, futureAppointments.Count);
    }
}