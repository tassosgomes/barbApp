using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICreateBarbershopServiceUseCase
{
    Task<BarbershopServiceOutput> ExecuteAsync(CreateBarbershopServiceInput input, CancellationToken cancellationToken);
}