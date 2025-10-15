// BarbApp.Infrastructure/Persistence/Repositories/AppointmentRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly BarbAppDbContext _context;

    public AppointmentRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Appointment>> GetFutureAppointmentsByBarberAsync(
        Guid barberId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.BarberId == barberId && a.StartTime > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Appointment>> GetAppointmentsByBarbeariaAndDateAsync(
        Guid barbeariaId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Appointments
            .Where(a => a.BarbeariaId == barbeariaId &&
                       a.StartTime >= startOfDay &&
                       a.StartTime < endOfDay)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        IEnumerable<Appointment> appointments,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        foreach (var appointment in appointments)
        {
            appointment.UpdateStatus(newStatus);
            _context.Appointments.Update(appointment);
        }

        // Don't call SaveChangesAsync here - let UnitOfWork handle it
    }
}