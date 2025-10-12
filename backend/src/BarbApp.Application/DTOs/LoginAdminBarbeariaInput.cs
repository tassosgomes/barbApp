// BarbApp.Application/DTOs/LoginAdminBarbeariaInput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de entrada para autenticação de administrador de barbearia
/// </summary>
public record LoginAdminBarbeariaInput
{
    /// <summary>
    /// Código único da barbearia (8 caracteres alfanuméricos maiúsculos)
    /// </summary>
    /// <example>ABC12345</example>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>
    /// Email do administrador da barbearia
    /// </summary>
    /// <example>admin@barbearia1.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Senha do administrador da barbearia
    /// </summary>
    /// <example>Admin@123</example>
    public string Senha { get; init; } = string.Empty;
}