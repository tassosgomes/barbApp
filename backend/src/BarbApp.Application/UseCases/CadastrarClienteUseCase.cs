// BarbApp.Application/UseCases/CadastrarClienteUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class CadastrarClienteUseCase : ICadastrarClienteUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public CadastrarClienteUseCase(
        IBarbershopRepository barbershopRepository,
        IClienteRepository clienteRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _barbershopRepository = barbershopRepository;
        _clienteRepository = clienteRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<CadastroClienteOutput> Handle(CadastrarClienteInput input, CancellationToken cancellationToken = default)
    {
        // 1. Validar se barbearia existe e está ativa
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.CodigoBarbearia, cancellationToken);
        if (barbearia == null || !barbearia.IsActive)
        {
            throw new BarbeariaNotFoundException(input.CodigoBarbearia);
        }

        // 2. Validar se telefone já está cadastrado nesta barbearia
        var clienteExistente = await _clienteRepository.ExisteAsync(barbearia.Id, input.Telefone, cancellationToken);
        if (clienteExistente)
        {
            throw new ClienteJaExisteException(input.Telefone);
        }

        // 3. Criar cliente
        var cliente = Cliente.Create(barbearia.Id, input.Nome, input.Telefone);

        // 4. Persistir cliente
        await _clienteRepository.AddAsync(cliente, cancellationToken);

        // 5. Gerar token JWT com contexto da barbearia
        var token = _jwtTokenGenerator.GenerateToken(
            userId: cliente.Id.ToString(),
            userType: "Cliente",
            email: cliente.Telefone, // usando telefone como email
            barbeariaId: barbearia.Id,
            barbeariaCode: barbearia.Code.Value
        );

        // 6. Retornar output com mapeamento manual
        return new CadastroClienteOutput
        {
            Token = token.Value,
            Cliente = new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone
            },
            Barbearia = new BarbeariaDto
            {
                Id = barbearia.Id,
                Nome = barbearia.Name,
                Codigo = barbearia.Code.Value
            }
        };
    }
}