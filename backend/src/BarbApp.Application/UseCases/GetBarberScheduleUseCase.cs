using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class GetBarberScheduleUseCase : IGetBarberScheduleUseCase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IBarberRepository _barberRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetBarberScheduleUseCase> _logger;

    public GetBarberScheduleUseCase(
        IAppointmentRepository appointmentRepository,
        IBarberRepository barberRepository,
        ICustomerRepository customerRepository,
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        ILogger<GetBarberScheduleUseCase> _logger)
    {
        _appointmentRepository = appointmentRepository;
        _barberRepository = barberRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        this._logger = _logger;
    }

    public async Task<BarberScheduleOutput> ExecuteAsync(DateTime date, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting barber schedule for date {Date}", date);

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

        // Get barber info
        var barber = await _barberRepository.GetByIdAsync(barberId, cancellationToken);
        if (barber == null || barber.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning("Barber {BarberId} not found or does not belong to barbearia {BarbeariaId}", barberId, barbeariaId);
            throw new BarberNotFoundException(barberId);
        }

        // Get appointments
        var appointments = await _appointmentRepository.GetByBarberAndDateAsync(
            barbeariaId.Value, barberId, date, cancellationToken);

        // Enrich with customer and service data
        var appointmentOutputs = new List<BarberAppointmentOutput>();
        foreach (var appointment in appointments.OrderBy(a => a.StartTime))
        {
            var customer = await _customerRepository.GetByIdAsync(appointment.CustomerId, cancellationToken);
            var service = await _serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);

            if (customer == null || service == null)
            {
                _logger.LogWarning("Customer {CustomerId} or Service {ServiceId} not found for appointment {AppointmentId}",
                    appointment.CustomerId, appointment.ServiceId, appointment.Id);
                continue; // Skip invalid appointments
            }

            appointmentOutputs.Add(new BarberAppointmentOutput(
                appointment.Id,
                customer.Name,
                service.Name,
                appointment.StartTime,
                appointment.EndTime,
                appointment.Status
            ));
        }

        _logger.LogInformation("Retrieved {Count} appointments for barber {BarberId} on date {Date}",
            appointmentOutputs.Count, barberId, date);

        return new BarberScheduleOutput(
            date,
            barber.Id,
            barber.Name,
            appointmentOutputs
        );
    }
}