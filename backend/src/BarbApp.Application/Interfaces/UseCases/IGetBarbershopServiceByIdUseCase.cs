using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IGetBarbershopServiceByIdUseCase
{
    Task<BarbershopServiceOutput> ExecuteAsync(Guid serviceId, CancellationToken cancellationToken);
}