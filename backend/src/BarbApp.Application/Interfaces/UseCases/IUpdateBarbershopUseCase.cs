using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IUpdateBarbershopUseCase
{
    Task<BarbershopOutput> ExecuteAsync(UpdateBarbershopInput input, CancellationToken cancellationToken);
}