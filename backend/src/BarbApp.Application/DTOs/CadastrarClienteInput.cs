// BarbApp.Application/DTOs/CadastrarClienteInput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de entrada para cadastro de novo cliente
/// </summary>
public record CadastrarClienteInput
{
    /// <summary>
    /// Código único da barbearia (8 caracteres alfanuméricos maiúsculos)
    /// </summary>
    /// <example>ABC12345</example>
    public string CodigoBarbearia { get; init; } = string.Empty;

    /// <summary>
    /// Nome completo do cliente
    /// </summary>
    /// <example>João Silva</example>
    public string Nome { get; init; } = string.Empty;

    /// <summary>
    /// Telefone do cliente (apenas números, formato brasileiro: 10 ou 11 dígitos)
    /// </summary>
    /// <example>11987654321</example>
    public string Telefone { get; init; } = string.Empty;
}