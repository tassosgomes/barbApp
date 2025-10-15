using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IGetBarberByIdUseCase
{
    Task<BarberOutput> ExecuteAsync(Guid barberId, CancellationToken cancellationToken);
}