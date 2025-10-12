// BarbApp.Application/DTOs/AuthResponse.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Resposta de autenticação bem-sucedida
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// Token JWT para autenticação em requisições subsequentes
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Tipo de usuário autenticado (AdminCentral, AdminBarbearia, Barbeiro, Cliente)
    /// </summary>
    /// <example>AdminBarbearia</example>
    public string TipoUsuario { get; init; } = string.Empty;

    /// <summary>
    /// ID da barbearia do usuário (null para AdminCentral)
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? BarbeariaId { get; init; }

    /// <summary>
    /// Nome da barbearia (vazio para AdminCentral)
    /// </summary>
    /// <example>Barbearia Premium</example>
    public string NomeBarbearia { get; init; } = string.Empty;

    /// <summary>
    /// Data e hora de expiração do token
    /// </summary>
    /// <example>2024-01-15T18:00:00Z</example>
    public DateTime ExpiresAt { get; init; }
}