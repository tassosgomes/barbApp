using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IAuthenticateBarbeiroUseCase
{
    Task<AuthResponse> ExecuteAsync(LoginBarbeiroInput input, CancellationToken cancellationToken = default);
}

