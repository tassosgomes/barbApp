using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ConfirmAppointmentUseCase : IConfirmAppointmentUseCase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmAppointmentUseCase> _logger;

    public ConfirmAppointmentUseCase(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<ConfirmAppointmentUseCase> _logger)
    {
        _appointmentRepository = appointmentRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        this._logger = _logger;
    }

    public async Task<AppointmentDetailsOutput> ExecuteAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var barbeariaId = _tenantContext.BarbeariaId;
        Guid barberId;
        try
        {
            barberId = Guid.Parse(_tenantContext.UserId);
        }
        catch
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("ID do barbeiro inválido");
        }

        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        _logger.LogInformation("Confirming appointment {AppointmentId} for barber {BarberId} in barbearia {BarbeariaId}", 
            appointmentId, barberId, barbeariaId.Value);

        // Get appointment
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
        {
            _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
            throw new AppointmentNotFoundException(appointmentId);
        }

        // Check ownership
        if (appointment.BarbeariaId != barbeariaId.Value || appointment.BarberId != barberId)
        {
            _logger.LogWarning("Appointment {AppointmentId} does not belong to barber {BarberId} in barbearia {BarbeariaId}",
                appointmentId, barberId, barbeariaId);
            throw new ForbiddenException("Acesso negado ao agendamento");
        }

        // Confirm appointment
        try
        {
            appointment.Confirm();
            _logger.LogInformation("Appointment {AppointmentId} confirmed successfully for barber {BarberId} in barbearia {BarbeariaId}", 
                appointmentId, barberId, barbeariaId.Value);

            // Record metric
            BarbAppMetrics.AppointmentStatusChangedCounter.WithLabels(barbeariaId.Value.ToString(), "Confirmed").Inc();
        }
        catch (InvalidAppointmentStatusTransitionException ex)
        {
            _logger.LogWarning("Failed to confirm appointment {AppointmentId}: {Message}", appointmentId, ex.Message);
            throw new ConflictException($"Não é possível confirmar o agendamento: {ex.Message}");
        }

        // Update
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        // Return updated details
        var customer = await _customerRepository.GetByIdAsync(appointment.CustomerId, cancellationToken);
        var service = await _serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);

        return new AppointmentDetailsOutput(
            appointment.Id,
            customer?.Name ?? "Cliente não encontrado",
            customer?.Telefone ?? "",
            service?.Name ?? "Serviço não encontrado",
            service?.Price ?? 0,
            service?.DurationMinutes ?? 0,
            appointment.StartTime,
            appointment.EndTime,
            appointment.Status,
            appointment.CreatedAt,
            appointment.ConfirmedAt,
            appointment.CancelledAt,
            appointment.CompletedAt
        );
    }
}