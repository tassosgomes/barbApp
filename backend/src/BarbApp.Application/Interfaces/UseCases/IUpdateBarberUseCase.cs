using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IUpdateBarberUseCase
{
    Task<BarberOutput> ExecuteAsync(UpdateBarberInput input, CancellationToken cancellationToken);
}