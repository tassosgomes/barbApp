// BarbApp.Application/UseCases/CadastrarClienteUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class CadastrarClienteUseCase : ICadastrarClienteUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<CadastrarClienteUseCase> _logger;

    public CadastrarClienteUseCase(
        IBarbershopRepository barbershopRepository,
        IClienteRepository clienteRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<CadastrarClienteUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _clienteRepository = clienteRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<CadastroClienteOutput> Handle(CadastrarClienteInput input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Iniciando cadastro de cliente. CodigoBarbearia: {CodigoBarbearia}, Telefone: {TelefoneMascarado}",
            input.CodigoBarbearia,
            MascararTelefone(input.Telefone));

        // 1. Validar se barbearia existe e está ativa
        var barbearia = await _barbershopRepository.GetByCodeAsync(input.CodigoBarbearia, cancellationToken);
        if (barbearia == null || !barbearia.IsActive)
        {
            _logger.LogWarning(
                "Cadastro falhou - Barbearia não encontrada ou inativa. Codigo: {CodigoBarbearia}",
                input.CodigoBarbearia);
            throw new BarbeariaNotFoundException(input.CodigoBarbearia);
        }

        // 2. Validar se telefone já está cadastrado nesta barbearia
        var clienteExistente = await _clienteRepository.ExisteAsync(barbearia.Id, input.Telefone, cancellationToken);
        if (clienteExistente)
        {
            _logger.LogWarning(
                "Cadastro falhou - Telefone já cadastrado. BarbeariaId: {BarbeariaId}, Telefone: {TelefoneMascarado}",
                barbearia.Id,
                MascararTelefone(input.Telefone));
            throw new ClienteJaExisteException(input.Telefone);
        }

        // 3. Criar cliente
        var cliente = Cliente.Create(barbearia.Id, input.Nome, input.Telefone);

        // 4. Persistir cliente
        await _clienteRepository.AddAsync(cliente, cancellationToken);

        _logger.LogInformation(
            "Cliente cadastrado com sucesso. ClienteId: {ClienteId}, BarbeariaId: {BarbeariaId}",
            cliente.Id,
            barbearia.Id);

        // Incrementar métrica de clientes cadastrados
        BarbAppMetrics.ClientesCadastradosCounter.WithLabels(barbearia.Id.ToString()).Inc();

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