// BarbApp.Application/UseCases/AuthenticateBarbeiroUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class AuthenticateBarbeiroUseCase : IAuthenticateBarbeiroUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IBarberRepository _repository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticateBarbeiroUseCase(
        IBarbershopRepository barbershopRepository,
        IBarberRepository repository,
        IJwtTokenGenerator tokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _barbershopRepository = barbershopRepository;
        _repository = repository;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginBarbeiroInput input, CancellationToken cancellationToken = default)
    {
        // Buscar barbeiro por email
        var barber = await _repository.GetByEmailGlobalAsync(input.Email, cancellationToken);

        if (barber == null || !_passwordHasher.Verify(input.Password, barber.PasswordHash))
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("E-mail ou senha inválidos");
        }

        // Buscar barbearia do barbeiro
        var barbearia = await _barbershopRepository.GetByIdAsync(barber.BarbeariaId, cancellationToken);

        if (barbearia == null || !barbearia.IsActive)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Barbearia inativa ou não encontrada");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: barber.Id.ToString(),
            userType: "Barbeiro",
            email: barber.Email,
            barbeariaId: barbearia.Id,
            barbeariaCode: barbearia.Code.Value
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "Barbeiro",
            BarbeariaId = barbearia.Id,
            NomeBarbearia = barbearia.Name,
            ExpiresAt = token.ExpiresAt
        };
    }
}
