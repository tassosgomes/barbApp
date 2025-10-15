using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IAppointmentRepository
{
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
