// BarbApp.Application/UseCases/LoginClienteUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class LoginClienteUseCase : ILoginClienteUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginClienteUseCase> _logger;

    public LoginClienteUseCase(
        IBarbershopRepository barbershopRepository,
        IClienteRepository clienteRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginClienteUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _clienteRepository = clienteRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<LoginClienteOutput> Handle(LoginClienteInput input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Tentativa de login de cliente. CodigoBarbearia: {CodigoBarbearia}, Telefone: {TelefoneMascarado}",
            input.CodigoBarbearia,
            MascararTelefone(input.Telefone));

        // 1. Validar se barbearia existe e está ativa
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.CodigoBarbearia, cancellationToken);
        if (barbearia == null || !barbearia.IsActive)
        {
            _logger.LogWarning(
                "Login falhou - Barbearia não encontrada ou inativa. Codigo: {CodigoBarbearia}",
                input.CodigoBarbearia);
            throw new BarbeariaNotFoundException(input.CodigoBarbearia);
        }

        // 2. Buscar cliente por telefone
        var cliente = await _clienteRepository.GetByTelefoneAsync(barbearia.Id, input.Telefone, cancellationToken);
        if (cliente == null)
        {
            _logger.LogWarning(
                "Login falhou - Telefone não cadastrado. BarbeariaId: {BarbeariaId}, Telefone: {TelefoneMascarado}",
                barbearia.Id,
                MascararTelefone(input.Telefone));
            throw new UnauthorizedException("Telefone não cadastrado. Faça seu primeiro agendamento!");
        }

        // 3. Validar nome (case-insensitive)
        if (!cliente.ValidarNomeLogin(input.Nome))
        {
            _logger.LogWarning(
                "Login falhou - Nome incorreto. ClienteId: {ClienteId}, BarbeariaId: {BarbeariaId}",
                cliente.Id,
                barbearia.Id);
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

        _logger.LogInformation(
            "Login de cliente realizado com sucesso. ClienteId: {ClienteId}, BarbeariaId: {BarbeariaId}",
            cliente.Id,
            barbearia.Id);

        // Incrementar métrica de logins bem-sucedidos
        BarbAppMetrics.LoginsClientesCounter.WithLabels(barbearia.Id.ToString(), "sucesso").Inc();

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

    /// <summary>
    /// Mascara o telefone para exibição em logs (LGPD)
    /// Ex: 11987654321 -> 11987****21
    /// </summary>
    private static string MascararTelefone(string telefone)
    {
        if (string.IsNullOrEmpty(telefone) || telefone.Length < 6)
            return "****";

        var inicio = telefone.Substring(0, 5);
        var fim = telefone.Substring(telefone.Length - 2);
        return $"{inicio}****{fim}";
    }
}