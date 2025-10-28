// BarbApp.Application/UseCases/AuthenticateClienteUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class AuthenticateClienteUseCase : IAuthenticateClienteUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticateClienteUseCase(
        IBarbershopRepository barbershopRepository,
        ICustomerRepository customerRepository,
        IClienteRepository clienteRepository,
        IJwtTokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _barbershopRepository = barbershopRepository;
        _customerRepository = customerRepository;
        _clienteRepository = clienteRepository;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginClienteInput input, CancellationToken cancellationToken = default)
    {
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.CodigoBarbearia, cancellationToken);

        if (barbearia == null || !barbearia.IsActive)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Código da barbearia inválido");
        }

        var customer = await _customerRepository.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, cancellationToken);

        if (customer == null)
        {
            // Criar novo cliente (Customer e Cliente)
            customer = Customer.Create(barbearia.Id, input.Telefone, input.Nome);
            await _customerRepository.AddAsync(customer, cancellationToken);
            
            // Também criar entidade Cliente para compatibilidade com agendamentos (usando mesmo ID)
            var cliente = Cliente.Create(customer.Id, barbearia.Id, input.Nome, input.Telefone);
            await _clienteRepository.AddAsync(cliente, cancellationToken);
            
            // Commit the transaction to save both entities
            await _unitOfWork.Commit(cancellationToken);
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
            barbeariaId: barbearia.Id,
            barbeariaCode: barbearia.Code.Value
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
