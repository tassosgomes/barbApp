// BarbApp.Application/UseCases/AuthenticateAdminBarbeariaUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class AuthenticateAdminBarbeariaUseCase : IAuthenticateAdminBarbeariaUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IAdminBarbeariaUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateAdminBarbeariaUseCase(
        IBarbershopRepository barbershopRepository,
        IAdminBarbeariaUserRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _barbershopRepository = barbershopRepository;
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginAdminBarbeariaInput input, CancellationToken cancellationToken = default)
    {
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.Codigo, cancellationToken);

        if (barbearia == null || !barbearia.IsActive)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Código da barbearia inválido");
        }

        var user = await _repository.GetByEmailAndBarbeariaIdAsync(input.Email, barbearia.Id, cancellationToken);

        if (user == null || !_passwordHasher.Verify(input.Senha, user.PasswordHash))
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Credenciais inválidas");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: user.Id.ToString(),
            userType: "AdminBarbearia",
            email: user.Email,
            barbeariaId: barbearia.Id,
            barbeariaCode: barbearia.Code.Value
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "AdminBarbearia",
            BarbeariaId = barbearia.Id,
            NomeBarbearia = barbearia.Name,
            CodigoBarbearia = barbearia.Code.Value,
            ExpiresAt = token.ExpiresAt
        };
    }
}
