// BarbApp.Infrastructure/Persistence/Repositories/AppointmentRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;

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
        // Since appointments are not yet implemented in the database,
        // return an empty list for now
        return new List<Appointment>();
    }

    public async Task UpdateStatusAsync(
        IEnumerable<Appointment> appointments,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        // Since appointments are not yet implemented in the database,
        // this is a no-op for now
        await Task.CompletedTask;
    }
}