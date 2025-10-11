// BarbApp.Application/UseCases/TrocarContextoBarbeiroUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class TrocarContextoBarbeiroUseCase
{
    private readonly IBarberRepository _repository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ITenantContext _tenantContext;

    public TrocarContextoBarbeiroUseCase(
        IBarberRepository repository,
        IJwtTokenGenerator tokenGenerator,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tokenGenerator = tokenGenerator;
        _tenantContext = tenantContext;
    }

    public async Task<AuthResponse> ExecuteAsync(TrocarContextoInput input, CancellationToken cancellationToken = default)
    {
        var userId = _tenantContext.UserId;
        var currentRole = _tenantContext.Role;

        if (string.IsNullOrEmpty(userId) || currentRole != "Barbeiro")
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Usuário não autenticado como barbeiro");
        }

        // Assumir que o identificador é telefone, já que não há email
        var telefone = userId; // since we used telefone as userId

        var barber = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, input.NovaBarbeariaId, cancellationToken);

        if (barber == null)
        {
            throw new BarbApp.Domain.Exceptions.NotFoundException("Barbeiro não encontrado na barbearia especificada");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: barber.Id.ToString(),
            userType: "Barbeiro",
            email: barber.Telefone,
            barbeariaId: barber.BarbeariaId
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "Barbeiro",
            BarbeariaId = barber.BarbeariaId,
            NomeBarbearia = barber.Barbearia.Name,
            ExpiresAt = token.ExpiresAt
        };
    }
}