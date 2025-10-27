// BarbApp.Application/UseCases/LoginClienteUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class LoginClienteUseCase : ILoginClienteUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginClienteUseCase(
        IBarbershopRepository barbershopRepository,
        IClienteRepository clienteRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _barbershopRepository = barbershopRepository;
        _clienteRepository = clienteRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginClienteOutput> Handle(LoginClienteInput input, CancellationToken cancellationToken = default)
    {
        // 1. Validar se barbearia existe e está ativa
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.CodigoBarbearia, cancellationToken);
        if (barbearia == null || !barbearia.IsActive)
        {
            throw new BarbeariaNotFoundException(input.CodigoBarbearia);
        }

        // 2. Buscar cliente por telefone
        var cliente = await _clienteRepository.GetByTelefoneAsync(barbearia.Id, input.Telefone, cancellationToken);
        if (cliente == null)
        {
            throw new UnauthorizedException("Telefone não cadastrado. Faça seu primeiro agendamento!");
        }

        // 3. Validar nome (case-insensitive)
        if (!cliente.ValidarNomeLogin(input.Nome))
        {
            throw new UnauthorizedException("Nome incorreto");
        }

        // 4. Gerar token JWT com contexto da barbearia
        var token = _jwtTokenGenerator.GenerateToken(
            userId: cliente.Id.ToString(),
            userType: "Cliente",
            email: cliente.Telefone, // usando telefone como email
            barbeariaId: barbearia.Id,
            barbeariaCode: barbearia.Code.Value
        );

        // 5. Retornar output com mapeamento manual
        return new LoginClienteOutput
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