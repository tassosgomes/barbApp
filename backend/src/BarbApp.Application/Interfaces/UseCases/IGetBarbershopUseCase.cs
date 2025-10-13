using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IGetBarbershopUseCase
{
    Task<BarbershopOutput> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}