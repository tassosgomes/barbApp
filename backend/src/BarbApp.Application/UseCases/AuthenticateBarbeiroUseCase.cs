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

    public AuthenticateBarbeiroUseCase(
        IBarbershopRepository barbershopRepository,
        IBarberRepository repository,
        IJwtTokenGenerator tokenGenerator)
    {
        _barbershopRepository = barbershopRepository;
        _repository = repository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginBarbeiroInput input, CancellationToken cancellationToken = default)
    {
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.Codigo, cancellationToken);

        if (barbearia == null || !barbearia.IsActive)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Código da barbearia inválido");
        }

        var barber = await _repository.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, cancellationToken);

        if (barber == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Barbeiro não encontrado");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: barber.Id.ToString(),
            userType: "Barbeiro",
            email: barber.Phone, // use Phone instead of Telefone
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
