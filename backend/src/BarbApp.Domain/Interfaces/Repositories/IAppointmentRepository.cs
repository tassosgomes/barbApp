using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Appointment>> GetByBarberAndDateAsync(
        Guid barbeariaId,
        Guid barberId,
        DateTime date,
        CancellationToken cancellationToken = default);

    Task InsertAsync(Appointment appointment, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    // Legacy methods - kept for backwards compatibility with existing use cases
    Task<List<Appointment>> GetFutureAppointmentsByBarberAsync(
        Guid barberId,
        CancellationToken cancellationToken = default);
    
    Task<List<Appointment>> GetAppointmentsByBarbeariaAndDateAsync(
        Guid barbeariaId,
        DateTime date,
        CancellationToken cancellationToken = default);
    
    Task UpdateStatusAsync(
        IEnumerable<Appointment> appointments,
        string newStatus,
        CancellationToken cancellationToken = default);
}
