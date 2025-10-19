// BarbApp.Application/DTOs/LoginBarbeiroInput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de entrada para autenticação de barbeiro
/// </summary>
public record LoginBarbeiroInput
{
    /// <summary>
    /// E-mail do barbeiro
    /// </summary>
    /// <example>barbeiro@example.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Senha do barbeiro
    /// </summary>
    /// <example>SenhaSegura123!</example>
    public string Password { get; init; } = string.Empty;
}