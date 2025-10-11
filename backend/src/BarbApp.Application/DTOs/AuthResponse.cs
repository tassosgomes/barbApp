// BarbApp.Application/DTOs/AuthResponse.cs
namespace BarbApp.Application.DTOs;

public record AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public string TipoUsuario { get; init; } = string.Empty;
    public Guid? BarbeariaId { get; init; }
    public string NomeBarbearia { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}