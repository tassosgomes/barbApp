using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICreateBarbershopUseCase
{
    Task<BarbershopOutput> ExecuteAsync(CreateBarbershopInput input, CancellationToken cancellationToken);
}