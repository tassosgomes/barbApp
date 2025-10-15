using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICreateBarberUseCase
{
    Task<BarberOutput> ExecuteAsync(CreateBarberInput input, CancellationToken cancellationToken);
}