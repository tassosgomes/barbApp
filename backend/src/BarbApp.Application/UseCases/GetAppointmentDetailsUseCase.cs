using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class GetAppointmentDetailsUseCase : IGetAppointmentDetailsUseCase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAppointmentDetailsUseCase> _logger;

    public GetAppointmentDetailsUseCase(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        ILogger<GetAppointmentDetailsUseCase> _logger)
    {
        _appointmentRepository = appointmentRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        this._logger = _logger;
    }

    public async Task<AppointmentDetailsOutput> ExecuteAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting appointment details for ID {AppointmentId}", appointmentId);

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

        // Get customer
        var customer = await _customerRepository.GetByIdAsync(appointment.CustomerId, cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found for appointment {AppointmentId}",
                appointment.CustomerId, appointmentId);
            throw new NotFoundException("Cliente não encontrado");
        }

        // Get service
        var service = await _serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);
        if (service == null)
        {
            _logger.LogWarning("Service {ServiceId} not found for appointment {AppointmentId}",
                appointment.ServiceId, appointmentId);
            throw new NotFoundException("Serviço não encontrado");
        }

        _logger.LogInformation("Appointment {AppointmentId} details retrieved successfully", appointmentId);

        return new AppointmentDetailsOutput(
            appointment.Id,
            customer.Name,
            customer.Telefone,
            service.Name,
            service.Price,
            service.DurationMinutes,
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