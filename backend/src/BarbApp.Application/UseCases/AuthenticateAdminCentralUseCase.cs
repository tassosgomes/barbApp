// BarbApp.Application/UseCases/AuthenticateAdminCentralUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class AuthenticateAdminCentralUseCase
{
    private readonly IAdminCentralUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateAdminCentralUseCase(
        IAdminCentralUserRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginAdminCentralInput input, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByEmailAsync(input.Email, cancellationToken);

        if (user == null || !_passwordHasher.Verify(input.Senha, user.PasswordHash))
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Credenciais inválidas");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: user.Id.ToString(),
            userType: "AdminCentral",
            email: user.Email,
            barbeariaId: null
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "AdminCentral",
            BarbeariaId = null,
            NomeBarbearia = string.Empty,
            ExpiresAt = token.ExpiresAt
        };
    }
}