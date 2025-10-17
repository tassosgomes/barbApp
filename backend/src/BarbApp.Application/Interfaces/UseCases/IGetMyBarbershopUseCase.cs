using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IGetMyBarbershopUseCase
{
    Task<BarbershopOutput> ExecuteAsync(CancellationToken cancellationToken);
}