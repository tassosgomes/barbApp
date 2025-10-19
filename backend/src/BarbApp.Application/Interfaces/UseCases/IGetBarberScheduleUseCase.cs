using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IGetBarberScheduleUseCase
{
    Task<BarberScheduleOutput> ExecuteAsync(DateTime date, CancellationToken cancellationToken);
}