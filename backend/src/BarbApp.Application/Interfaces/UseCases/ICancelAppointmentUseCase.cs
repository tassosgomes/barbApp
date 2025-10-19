using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICancelAppointmentUseCase
{
    Task<AppointmentDetailsOutput> ExecuteAsync(Guid appointmentId, CancellationToken cancellationToken);
}