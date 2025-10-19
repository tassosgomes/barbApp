using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IConfirmAppointmentUseCase
{
    Task<AppointmentDetailsOutput> ExecuteAsync(Guid appointmentId, CancellationToken cancellationToken);
}