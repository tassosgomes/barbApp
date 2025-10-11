// BarbApp.Application/Interfaces/IAuthenticationService.cs
using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationOutput> AuthenticateAdminCentralAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthenticationOutput> AuthenticateAdminBarbeariaAsync(
        string codigo,
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthenticationOutput> AuthenticateBarbeiroAsync(
        string codigo,
        string telefone,
        CancellationToken cancellationToken);

    Task<AuthenticationOutput> AuthenticateClienteAsync(
        string codigo,
        string telefone,
        string nome,
        CancellationToken cancellationToken);
}