using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IAuthenticateAdminBarbeariaUseCase
{
    Task<AuthResponse> ExecuteAsync(LoginAdminBarbeariaInput input, CancellationToken cancellationToken = default);
}

