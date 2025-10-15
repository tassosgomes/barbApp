using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IUpdateBarbershopServiceUseCase
{
    Task<BarbershopServiceOutput> ExecuteAsync(UpdateBarbershopServiceInput input, CancellationToken cancellationToken);
}