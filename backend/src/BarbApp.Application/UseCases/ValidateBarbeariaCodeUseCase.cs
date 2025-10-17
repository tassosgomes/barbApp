using System.Text.RegularExpressions;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

/// <summary>
/// Use case para validar código de barbearia e retornar informações básicas.
/// Usado na tela de login para verificar se o código na URL é válido.
/// </summary>
public class ValidateBarbeariaCodeUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private static readonly Regex CodeRegex = new("^[A-Z0-9]{8}$", RegexOptions.Compiled);

    public ValidateBarbeariaCodeUseCase(IBarbershopRepository barbershopRepository)
    {
        _barbershopRepository = barbershopRepository;
    }

    public async Task<ValidateBarbeariaCodeResponse> ExecuteAsync(string codigo, CancellationToken cancellationToken = default)
    {
        // Validar formato do código
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ValidationException("Código é obrigatório");

        if (!CodeRegex.IsMatch(codigo))
            throw new ValidationException("Código inválido. Deve conter 8 caracteres alfanuméricos maiúsculos");

        // Buscar barbearia pelo código
        var barbearia = await _barbershopRepository.GetByCodeAsync(codigo, cancellationToken);
        
        if (barbearia == null)
            throw new NotFoundException("Barbearia não encontrada");

        // Validar se barbearia está ativa
        if (!barbearia.IsActive)
            throw new ForbiddenException("Barbearia temporariamente indisponível");

        // Retornar apenas dados básicos (sem informações sensíveis)
        return new ValidateBarbeariaCodeResponse
        {
            Id = barbearia.Id,
            Nome = barbearia.Name,
            Codigo = barbearia.Code.Value,
            IsActive = barbearia.IsActive
        };
    }
}
