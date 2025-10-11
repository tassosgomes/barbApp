// BarbApp.Application/UseCases/AuthenticateClienteUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class AuthenticateClienteUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly ICustomerRepository _repository;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateClienteUseCase(
        IBarbershopRepository barbershopRepository,
        ICustomerRepository repository,
        IJwtTokenGenerator tokenGenerator)
    {
        _barbershopRepository = barbershopRepository;
        _repository = repository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginClienteInput input, CancellationToken cancellationToken = default)
    {
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.Codigo, cancellationToken);

        if (barbearia == null || !barbearia.IsActive)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Código da barbearia inválido");
        }

        var customer = await _repository.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, cancellationToken);

        if (customer == null)
        {
            // Criar novo cliente
            customer = Customer.Create(barbearia.Id, input.Telefone, input.Nome);
            await _repository.AddAsync(customer, cancellationToken);
        }
        else
        {
            // Verificar se o nome corresponde
            if (!string.Equals(customer.Name, input.Nome, StringComparison.OrdinalIgnoreCase))
            {
                throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Nome não corresponde ao telefone cadastrado");
            }
        }

        var token = _tokenGenerator.GenerateToken(
            userId: customer.Id.ToString(),
            userType: "Cliente",
            email: customer.Telefone, // use telefone
            barbeariaId: barbearia.Id
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "Cliente",
            BarbeariaId = barbearia.Id,
            NomeBarbearia = barbearia.Name,
            ExpiresAt = token.ExpiresAt
        };
    }
}