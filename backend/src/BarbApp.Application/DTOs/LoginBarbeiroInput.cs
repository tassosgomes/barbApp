// BarbApp.Application/DTOs/LoginBarbeiroInput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de entrada para autenticação de barbeiro
/// </summary>
public record LoginBarbeiroInput
{
    /// <summary>
    /// Código único da barbearia onde o barbeiro trabalha (8 caracteres alfanuméricos maiúsculos)
    /// </summary>
    /// <example>ABC12345</example>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>
    /// Telefone do barbeiro (apenas números, formato brasileiro: 10 ou 11 dígitos)
    /// </summary>
    /// <example>11987654321</example>
    public string Telefone { get; init; } = string.Empty;
}