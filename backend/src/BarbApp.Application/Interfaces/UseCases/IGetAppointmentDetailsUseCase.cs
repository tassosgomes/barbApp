using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IGetAppointmentDetailsUseCase
{
    Task<AppointmentDetailsOutput> ExecuteAsync(Guid appointmentId, CancellationToken cancellationToken);
}