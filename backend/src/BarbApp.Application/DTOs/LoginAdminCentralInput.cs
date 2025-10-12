// BarbApp.Application/DTOs/LoginAdminCentralInput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de entrada para autenticação de administrador central
/// </summary>
public record LoginAdminCentralInput
{
    /// <summary>
    /// Email do administrador central
    /// </summary>
    /// <example>admin@barbapp.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Senha do administrador central
    /// </summary>
    /// <example>Admin@123</example>
    public string Senha { get; init; } = string.Empty;
}