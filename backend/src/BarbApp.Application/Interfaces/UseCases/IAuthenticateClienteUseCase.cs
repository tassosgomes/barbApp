using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IAuthenticateClienteUseCase
{
    Task<AuthResponse> ExecuteAsync(LoginClienteInput input, CancellationToken cancellationToken = default);
}

