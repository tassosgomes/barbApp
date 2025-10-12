using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IAuthenticateAdminCentralUseCase
{
    Task<AuthResponse> ExecuteAsync(LoginAdminCentralInput input, CancellationToken cancellationToken = default);
}

