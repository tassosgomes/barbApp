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

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Barber)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Appointment>> GetByBarberAndDateAsync(
        Guid barbeariaId,
        Guid barberId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Barber)
            .Where(a => a.BarbeariaId == barbeariaId &&
                       a.BarberId == barberId &&
                       a.StartTime >= startOfDay &&
                       a.StartTime < endOfDay)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _context.Appointments.Update(appointment);
        await Task.CompletedTask;
    }

    // Legacy methods - kept for backwards compatibility
    public async Task<List<Appointment>> GetFutureAppointmentsByBarberAsync(
        Guid barberId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.BarberId == barberId && a.StartTime > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Appointment>> GetAppointmentsByBarbeariaAndDateAsync(
        Guid barbeariaId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date.ToUniversalTime();
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.BarbeariaId == barbeariaId &&
                       a.StartTime >= startOfDay &&
                       a.StartTime < endOfDay)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        IEnumerable<Appointment> appointments,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        foreach (var appointment in appointments)
        {
            // Map string status to enum and update using domain methods
            switch (newStatus.ToLower())
            {
                case "confirmed":
                    if (appointment.Status == Domain.Enums.AppointmentStatus.Pending)
                        appointment.Confirm();
                    break;
                case "cancelled":
                    appointment.Cancel();
                    break;
                case "completed":
                    if (appointment.Status == Domain.Enums.AppointmentStatus.Confirmed)
                        appointment.Complete();
                    break;
            }
            _context.Appointments.Update(appointment);
        }

        await Task.CompletedTask;
        // Don't call SaveChangesAsync here - let UnitOfWork handle it
    }
}