using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICompleteAppointmentUseCase
{
    Task<AppointmentDetailsOutput> ExecuteAsync(Guid appointmentId, CancellationToken cancellationToken);
}