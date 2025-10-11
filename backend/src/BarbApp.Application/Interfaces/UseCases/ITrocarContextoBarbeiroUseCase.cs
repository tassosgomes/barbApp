using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ITrocarContextoBarbeiroUseCase
{
    Task<AuthResponse> ExecuteAsync(TrocarContextoInput input, CancellationToken cancellationToken = default);
}

