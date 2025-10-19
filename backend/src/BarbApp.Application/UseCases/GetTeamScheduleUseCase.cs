using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BarbApp.Application.UseCases;

public class GetTeamScheduleUseCase : IGetTeamScheduleUseCase
{
    private readonly ITenantContext _tenantContext;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IBarberRepository _barberRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<GetTeamScheduleUseCase> _logger;

    public GetTeamScheduleUseCase(
        ITenantContext tenantContext,
        IAppointmentRepository appointmentRepository,
        IBarberRepository barberRepository,
        ICustomerRepository customerRepository,
        ILogger<GetTeamScheduleUseCase> logger)
    {
        _tenantContext = tenantContext;
        _appointmentRepository = appointmentRepository;
        _barberRepository = barberRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<TeamScheduleOutput> ExecuteAsync(DateTime date, Guid? barberId, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Getting team schedule for date {Date}, barberId: {BarberId}", date, barberId);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        var appointments = await GetAppointmentsAsync(barbeariaId.Value, date, barberId, cancellationToken);
        var appointmentOutputs = await MapToAppointmentOutputsAsync(appointments, cancellationToken);

        stopwatch.Stop();
        BarbAppMetrics.ScheduleRetrievalDuration.WithLabels(barbeariaId.Value.ToString()).Observe(stopwatch.Elapsed.TotalSeconds);

        _logger.LogInformation("Team schedule retrieved for date {Date} with {Count} appointments in {Duration}ms", date, appointmentOutputs.Count, stopwatch.ElapsedMilliseconds);

        return new TeamScheduleOutput(appointmentOutputs);
    }

    private async Task<List<Appointment>> GetAppointmentsAsync(Guid barbeariaId, DateTime date, Guid? barberId, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByBarbeariaAndDateAsync(
            barbeariaId, date, cancellationToken);

        if (barberId.HasValue)
        {
            appointments = appointments.Where(a => a.BarberId == barberId.Value).ToList();
        }

        return appointments;
    }

    private async Task<List<AppointmentOutput>> MapToAppointmentOutputsAsync(List<Appointment> appointments, CancellationToken cancellationToken)
    {
        if (!appointments.Any())
        {
            return new List<AppointmentOutput>();
        }

        var barberIds = appointments.Select(a => a.BarberId).Distinct().ToList();
        var customerIds = appointments.Select(a => a.CustomerId).Distinct().ToList();

        var barbers = await GetBarbersAsync(barberIds, cancellationToken);
        var customers = await GetCustomersAsync(customerIds, cancellationToken);

        return appointments.Select(appointment => MapToAppointmentOutput(appointment, barbers, customers)).ToList();
    }

    private async Task<Dictionary<Guid, Barber>> GetBarbersAsync(List<Guid> barberIds, CancellationToken cancellationToken)
    {
        var barbers = new Dictionary<Guid, Barber>();
        foreach (var barberId in barberIds)
        {
            var barber = await _barberRepository.GetByIdAsync(barberId, cancellationToken);
            if (barber != null)
            {
                barbers[barberId] = barber;
            }
        }
        return barbers;
    }

    private async Task<Dictionary<Guid, Customer>> GetCustomersAsync(List<Guid> customerIds, CancellationToken cancellationToken)
    {
        var customers = new Dictionary<Guid, Customer>();
        foreach (var customerId in customerIds)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer != null)
            {
                customers[customerId] = customer;
            }
        }
        return customers;
    }

    private AppointmentOutput MapToAppointmentOutput(Appointment appointment, Dictionary<Guid, Barber> barbers, Dictionary<Guid, Customer> customers)
    {
        var barberName = barbers.TryGetValue(appointment.BarberId, out var barber) ? barber.Name : "Unknown Barber";
        var customerName = customers.TryGetValue(appointment.CustomerId, out var customer) ? customer.Name : "Unknown Customer";

        return new AppointmentOutput(
            appointment.Id,
            appointment.BarberId,
            barberName,
            appointment.CustomerId,
            customerName,
            appointment.StartTime,
            appointment.EndTime,
            appointment.ServiceName,
            appointment.StatusString
        );
    }
}